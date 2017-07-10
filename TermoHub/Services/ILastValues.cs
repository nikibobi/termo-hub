namespace TermoHub.Services
{
    public interface ILastValues
    {
        double GetDeviceLastValuesAverage(int deviceId);
        double GetSensorLastValue(int deviceId, int sensorId);
        void SetSensorLastValue(int deviceId, int sensorId, double value);
    }
}
