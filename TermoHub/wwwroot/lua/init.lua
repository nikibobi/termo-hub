ds = require('ds18b20')

wifi.setmode(wifi.STATION)
wifi.sta.autoconnect(1)

url = 'http://192.168.1.52:53640/new'
device = node.chipid()
ds.setup(4)
addrs = ds.addrs()

local job = tmr.create()
job:register(5000, tmr.ALARM_AUTO, function()
    for i, addr in ipairs(addrs) do
        value = ds.read(addr)
        sensor = addr:byte(3) * 256 + addr:byte(4)
        data = string.format('{"DeviceId":%i,"SensorId":%i,"Value":%f}', device, sensor, value)
        http.post(url, 'Content-Type: application/json\r\n', data, nil)
    end
end)
job:start()