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

#define ONEWIRE_PIN D4
#define DELAY 5000
#define HOST "212.25.35.65"
#define PORT 5000
#define SSID "TermoHub"
#define PASS "12345678"

OneWire oneWire(ONEWIRE_PIN);
DallasTemperature ds(&oneWire);
DeviceAddress* addrs;
int n;
int deviceId;
int sleep = DELAY / 1000;

void setup() {
  Serial.begin(9600);
  WiFiManager wifiManager;
  wifiManager.setDebugOutput(false);
  wifiManager.autoConnect(SSID, PASS);

  Serial.println("IP address: " + WiFi.localIP());

  deviceId = ESP.getChipId();
  Serial.println("Device Id: " + String(deviceId));

  setupDallasTemperatures();
}

void setupDallasTemperatures() {
  ds.begin();
  n = ds.getDeviceCount();
  addrs = new DeviceAddress[n];
  for (int i = 0; i < n; i++) {
    ds.getAddress(addrs[i], i);
  }
  if (n > 0) {
    Serial.println(String(n) + " devices found");
  }
}

void loop() {
  if (WiFi.status() != WL_CONNECTED) {
    return;
  }

  readDallasTemperatures();

  delay(sleep * 1000); //ESP.deepSleep(sleep * 1000 * 1000);
}

void readDallasTemperatures() {
  for (int i = 0; i < n; i++) {
    ds.requestTemperaturesByAddress(addrs[i]);
    float value = ds.getTempC(addrs[i]);
    if (value == -127) {
      continue;
    }
    int sensorId = addrs[i][2] << 8 + addrs[i][3];
    sendRequest(sensorId, value);
  }
}

void sendRequest(int sensorId, float value) {
  HTTPClient http;
  http.begin(HOST, PORT, "/new");
  http.addHeader("Content-Type", "application/json");
  String payload = buildPayload(sensorId, value);
  int code = http.POST(payload);
  if (code > 0) {
    Serial.print("Status: ");
    Serial.println(code);
    if (code == HTTP_CODE_OK) {
      Serial.println("POST " + payload);
      String response = http.getString();
      Serial.println("sleep " + response + " sec");
      sleep = response.toInt();
    }
  } else {
    Serial.print("Error: ");
    Serial.println(http.errorToString(code));
  }
  http.end();
}

// {"DeviceId":%i,"SensorId":%i,"Value":%f}
String buildPayload(int sensorId, float value) {
  String result;
  result += "{\"DeviceId\":";
  result += String(deviceId);
  result += ",\"SensorId\":";
  result += String(sensorId);
  result += ",\"Value\":";
  result += String(value);
  result += "}";
  return result;
}