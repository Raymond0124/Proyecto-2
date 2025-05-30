using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Proyecto
{
    public enum PowerType
    {
        ForcePush,
        Shield,
        AirJump
    }

    public class Power
    {
        public PowerType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public bool IsActive { get; set; }
        public int RemainingTime { get; set; }

        public Power(PowerType type)
        {
            Type = type;
            IsActive = false;
            RemainingTime = 0;

            switch (type)
            {
                case PowerType.ForcePush:
                    Name = "Force Push";
                    Description = "Empuja jugadores con fuerza";
                    Duration = 1; // Uso instantáneo
                    break;
                case PowerType.Shield:
                    Name = "Shield";
                    Description = "Protege de empujones";
                    Duration = 300; // 5 segundos a 60 FPS
                    break;
                case PowerType.AirJump:
                    Name = "Air Jump";
                    Description = "Salto de emergencia en el aire";
                    Duration = 1; // Uso único
                    break;
            }
        }

        public void Activate()
        {
            IsActive = true;
            RemainingTime = Duration;
        }

        public void Update()
        {
            if (IsActive && RemainingTime > 0)
            {
                RemainingTime--;
                if (RemainingTime <= 0)
                {
                    IsActive = false;
                }
            }
        }

        public bool CanUse()
        {
            switch (Type)
            {
                case PowerType.ForcePush:
                    return true; // Siempre se puede usar (se consume al usarse)
                case PowerType.Shield:
                    return !IsActive; // Solo si no está activo
                case PowerType.AirJump:
                    return true; // Verificación adicional en UseAirJump
                default:
                    return false;
            }
        }
    }

    public class PowerManager
    {
        private List<PowerType> availablePowers = new List<PowerType>
        {
            PowerType.ForcePush,
            PowerType.Shield,
            PowerType.AirJump
        };

        private Random random = new Random();

        public Power GetRandomPower()
        {
            var randomType = availablePowers[random.Next(availablePowers.Count)];
            return new Power(randomType);
        }

        public static void DrawPowerIcon(Graphics g, Power power, int x, int y, int size = 30)
        {
            Rectangle iconRect = new Rectangle(x, y, size, size);

            // CORREGIDO: Mejor lógica de colores para visibilidad
            Color bgColor;
            Color borderColor = Color.Black;

            if (power.Type == PowerType.AirJump)
            {
                bgColor = Color.LightGreen; // AirJump siempre visible en verde
                borderColor = Color.DarkGreen;
            }
            else if (power.IsActive)
            {
                bgColor = Color.Gold; // Poderes activos en dorado
                borderColor = Color.Orange;
            }
            else
            {
                bgColor = Color.White; // Poderes disponibles en blanco
                borderColor = Color.Black;
            }

            // Dibujar fondo del ícono
            g.FillEllipse(new SolidBrush(bgColor), iconRect);
            g.DrawEllipse(new Pen(borderColor, 2), iconRect);

            // MEJORADO: Iconos más descriptivos
            using (Font iconFont = new Font("Arial", size / 4, FontStyle.Bold))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                string iconText = "";
                Color textColor = Color.Black;

                switch (power.Type)
                {
                    case PowerType.ForcePush:
                        iconText = "⚡"; // Usar símbolo de rayo si no funciona, usar "FP"
                        if (iconText == "⚡") // Fallback si el símbolo no se muestra
                            iconText = "FP";
                        textColor = Color.DarkRed;
                        break;
                    case PowerType.Shield:
                        iconText = "🛡"; // Usar símbolo de escudo si no funciona, usar "SH"
                        if (iconText == "🛡") // Fallback
                            iconText = "SH";
                        textColor = Color.DarkBlue;
                        break;
                    case PowerType.AirJump:
                        iconText = "↑"; // Flecha hacia arriba
                        textColor = Color.DarkGreen;
                        break;
                }

                g.DrawString(iconText, iconFont, new SolidBrush(textColor), iconRect, sf);
            }

            // MEJORADO: Mostrar tiempo restante para Shield
            if (power.IsActive && power.Type == PowerType.Shield && power.RemainingTime > 0)
            {
                int secondsLeft = Math.Max(1, power.RemainingTime / 60);
                using (Font timeFont = new Font("Arial", 8, FontStyle.Bold))
                {
                    // Fondo blanco para el texto del tiempo
                    string timeText = secondsLeft.ToString();
                    SizeF textSize = g.MeasureString(timeText, timeFont);
                    Rectangle textBg = new Rectangle(x + size - 15, y + size - 15, 15, 15);
                    g.FillRectangle(Brushes.White, textBg);
                    g.DrawRectangle(Pens.Black, textBg);
                    g.DrawString(timeText, timeFont, Brushes.Red, x + size - 12, y + size - 12);
                }
            }

            // AGREGADO: Indicador visual especial para poderes listos para usar
            if (power.CanUse() && !power.IsActive)
            {
                // Pequeño destello en la esquina superior derecha
                Rectangle glint = new Rectangle(x + size - 8, y, 8, 8);
                g.FillEllipse(Brushes.Yellow, glint);
                g.DrawEllipse(Pens.Gold, glint);
            }
        }


        public static void ApplyForcePush(Player pusher, Player target, int pushForce = 15)
        {
            if (target.HasShieldActive())
            {
                // Efecto visual cuando el shield bloquea
                target.PushEffectTime = 20;
                return;
            }

            // Calcular dirección del empujón
            int pushDirection = pusher.X < target.X ? 1 : -1;

            // Aplicar fuerza horizontal y vertical
            target.SpeedX = pushDirection * pushForce;
            target.SpeedY = -8; // Empujón hacia arriba también

            // Efecto visual de empujón
            target.PushEffectTime = 40; // Más duración para mejor visibilidad
        }

        public static bool CanAirJump(Player player)
        {
            return !player.IsOnGround() && player.HasAirJumpPower();
        }
    }
}