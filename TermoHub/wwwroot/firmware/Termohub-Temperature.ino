#include <FS.h>
#include <ESP.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
// wifi manager
#include <DNSServer.h>
#include <ESP8266WebServer.h>
#include <WiFiManager.h>
// dallas sensors
#include <OneWire.h>
#include <DallasTemperature.h>
// json
#include <ArduinoJson.h>

#define SAVE_FILE "/credentials.json"
#define ONEWIRE_PIN D4
#define DELAY 5000
#define HOST "212.25.35.65"
#define PORT 5000
#define SSID "TermoHub"
#define PASS "12345678"

bool saveRequested = false;
String token;
OneWire oneWire(ONEWIRE_PIN);
DallasTemperature ds(&oneWire);
DeviceAddress* addrs;
int n;
int deviceId;
int sleep = DELAY;

void setup() {
  Serial.begin(9600);

  char username[50] = "";
  char password[50] = "";

  loadCredentials(username, password);

  WiFiManagerParameter username_parameter("username", "username", username, 50);
  WiFiManagerParameter password_parameter("password", "password", password, 50);
  WiFiManager wifiManager;
  wifiManager.setDebugOutput(false);
  wifiManager.setSaveConfigCallback(saveConfig);
  wifiManager.addParameter(&username_parameter);
  wifiManager.addParameter(&password_parameter);
  wifiManager.autoConnect(SSID, PASS);

  strcpy(username, username_parameter.getValue());
  strcpy(password, password_parameter.getValue());

  if (saveRequested) {
    saveCredentials(username, password);
  }

  Serial.println("IP address: " + WiFi.localIP());
  deviceId = ESP.getChipId();
  Serial.println("Device Id: " + String(deviceId));

  fetchToken(username, password);
  
  setupDallasTemperatures();
}

void saveConfig() {
  saveRequested = true;
}

void loadCredentials(char* username, char* password) {
  if (SPIFFS.begin()) {
    if (SPIFFS.exists(SAVE_FILE)) {
      File file = SPIFFS.open(SAVE_FILE, "r");
      if (file) {
        size_t size = file.size();
        std::unique_ptr<char[]> buf(new char[size]);
        file.readBytes(buf.get(), size);
        
        DynamicJsonBuffer jsonBuffer;
        JsonObject& json = jsonBuffer.parseObject(buf.get());

        if (json.success()) {
          strcpy(username, json["Username"]);
          strcpy(password, json["Password"]);
        }
      }
      file.close();
    }
  }
}

void saveCredentials(char* username, char* password) {
  File file = SPIFFS.open(SAVE_FILE, "w");
  if (!file) {
    Serial.println("Failed to create credentials file");
  } else {
    DynamicJsonBuffer jsonBuffer;
    JsonObject& json = jsonBuffer.createObject();
    json["Username"] = username;
    json["Password"] = password;
    json.printTo(file);
  }
  file.close();
}

void fetchToken(char* username, char* password) {
  if (sendRequest("/token", "", tokenPayload(username, password), token)) {
    Serial.println("token: " + token);
  } else {
    Serial.println("Unable to fetch token");
    ESP.reset();
  }
}

String tokenPayload(char* username, char* password) {
  DynamicJsonBuffer jsonBuffer;
  JsonObject& json = jsonBuffer.createObject();
  json["Username"] = username;
  json["Password"] = password;
  String result;
  json.printTo(result);
  return result;
}

void setupDallasTemperatures() {
  ds.begin();
  n = ds.getDeviceCount();
  addrs = new DeviceAddress[n];
  for (int i = 0; i < n; i++) {
    ds.getAddress(addrs[i], i);
  }
  if (n > 0) {
    Serial.println(String(n) + " sensors found");
  }
}

void loop() {
  if (WiFi.status() != WL_CONNECTED) {
    return;
  }

  readDallasTemperatures();

  delay(sleep); //ESP.deepSleep(sleep * 1000);
}

void readDallasTemperatures() {
  for (int i = 0; i < n; i++) {
    int sensorId = addrs[i][2] << 8 + addrs[i][3];
    if (!ds.isConnected(addrs[i])) {
      Serial.println("Sensor " + String(sensorId) + " disconnected!");
      continue;
    }
    ds.requestTemperaturesByAddress(addrs[i]);
    float value = ds.getTempC(addrs[i]);
    if (value == -127 || value == 85) {
      continue;
    }
    String response;
    if (sendRequest("/new", token, valuePayload(sensorId, value), response)) {
      Serial.println("sleep " + response + " sec");
      sleep = response.toInt() * 1000;
    }
  }
}

// {"DeviceId":%i,"SensorId":%i,"Value":%f}
String valuePayload(int sensorId, float value) {
  DynamicJsonBuffer jsonBuffer;
  JsonObject& json = jsonBuffer.createObject();
  json["DeviceId"] = deviceId;
  json["SensorId"] = sensorId;
  json["Value"] = value;
  String result;
  json.printTo(result);
  return result;
}

bool sendRequest(String url, String token, String payload, String& response) {
  HTTPClient http;
  http.begin(HOST, PORT, url);
  http.addHeader("Content-Type", "application/json");
  if (token != "") {
    http.addHeader("Authorization", "Bearer " + token); 
  }
  int code = http.POST(payload);
  bool success = false;
  if (code > 0) {
    Serial.print("Status: ");
    Serial.println(code);
    if (code == HTTP_CODE_OK) {
      Serial.println("POST " + payload);
      response = http.getString();
      success = true;
    }
  } else {
    Serial.print("Error: ");
    Serial.println(http.errorToString(code));
  }
  http.end();
  return success;
}
