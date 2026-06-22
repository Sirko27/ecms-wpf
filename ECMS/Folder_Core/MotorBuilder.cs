using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_Core
{
    public class MotorBuilder
    {
        private readonly MotorDevice _motor;

        public MotorBuilder(MotorDevice motor)
        {
            _motor = motor ?? throw new ArgumentNullException(nameof(motor));
        }

        public void Apply(MotorConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _motor.Name = config.Name;
            _motor.MaxSpeed = config.MaxSpeed;
        }
    }
}
