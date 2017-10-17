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
// bmp 820 sensor
#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BMP280.h>
// am 2320 sensor
#include <AM2320.h>
// lcd 16x2 i2c
#include <LiquidCrystal_I2C.h>

#define ONEWIRE_PIN D4
#define I2C_ADDRESS 0x76
#define I2C_ID_OFFSET 65535
#define DELAY 5000
#define HOST "212.25.35.65"
#define PORT 5000
#define SSID "TermoHub"
#define PASS "12345678"
#define LCD_ADDRESS 0x27

OneWire oneWire(ONEWIRE_PIN);
DallasTemperature ds(&oneWire);
DeviceAddress* addrs;
int n;
bool bmpFound;
Adafruit_BMP280 bmp;
AM2320 am;
LiquidCrystal_I2C lcd(LCD_ADDRESS, 16, 2);
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

  setupLCD();
  setupDallasTemperatures();
  setupBMP280Sensors();
}

void setupLCD() {
  lcd.init();
  lcd.backlight();
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

void setupBMP280Sensors() {
  bmpFound = bmp.begin(I2C_ADDRESS);
  if (bmpFound) {
    Serial.println("BMP280 sensor found");
  }
}

void loop() {
  if (WiFi.status() != WL_CONNECTED) {
    return;
  }

  readDallasTemperatures();
  readBMP280Sensors();
  readAM2320Sensors();
  
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
    lcd.setCursor(0, i % 2);
    lcd.print("T" + String(i) + " " + String(value, 2) + String((char)223) + "C");
    sendRequest(sensorId, value);
  }
}

void readBMP280Sensors() {
  if (!bmpFound) {
    return;
  }
  // temperature
  int temperatureId = I2C_ID_OFFSET + (1 << 4);
  float temperature = bmp.readTemperature();
  sendRequest(temperatureId, temperature);
  // pressure
  int pressureId = I2C_ID_OFFSET + (1 << 5);
  float pressure = bmp.readPressure();
  sendRequest(pressureId, pressure);
  // altitude
  int altitudeId = I2C_ID_OFFSET + (1 << 6);
  float altitude = bmp.readAltitude();
  sendRequest(altitudeId, altitude);
}

void readAM2320Sensors() {
  if (am.Read() != 0) {
    return;
  }
  // humidity
  int humidityId = I2C_ID_OFFSET + (1 << 7);
  float humidity = am.h;
  sendRequest(humidityId, humidity);
  // temperature
  int temperatureId = I2C_ID_OFFSET + (1 << 8);
  float temperature = am.t;
  sendRequest(temperatureId, temperature);
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