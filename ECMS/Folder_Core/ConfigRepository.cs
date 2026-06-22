using ECMS.Folder_Core.Folder_Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ECMS.Folder_Core
{
    public static class ConfigRepository
    {
        private const string FilePath = "ConfigRepository.json";

        public static void Save(IEnumerable<IDevice> devices)
        {
            var config = devices.Select(d => new DeviceSaveData
            {
                Id = d.ID,
                Name = d.Name,
                MaxSpeed = d.MaxSpeed,
                Type = d is MotorDevice ? "Motor" : "Pump",
                LinkedPumpNames = d is MotorDevice motor ? motor.LinkedPumps.Select(p => p.Name).ToList() : new List<string>()
            }).ToList();

            string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        public static List<DeviceSaveData> Load()
        {
            if (!File.Exists(FilePath))
                return new List<DeviceSaveData>();

            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<DeviceSaveData>>(json) ?? new List<DeviceSaveData>();
        }
    }

    public class DeviceSaveData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int MaxSpeed { get; set; }
        public string Type { get; set; }
        public List<string> LinkedPumpNames { get; set; } = new List<string>();
    }
}
