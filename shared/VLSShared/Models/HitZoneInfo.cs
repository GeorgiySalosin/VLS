using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLSShared.Enums;

namespace VLSShared.Models
{
    public sealed record HitZoneInfo
    {
        public int BaseDamage { get; init; }
        public double FXScale { get; init; }

        
        private HitZone HitZone { get; init; }

        private HitZoneInfo(HitZone hitZone, int baseDamage, double fxScale)
        {
            HitZone = hitZone;
            BaseDamage = baseDamage;
            FXScale = fxScale;
        }

        // Static instances
        public static readonly HitZoneInfo None = new(HitZone.None, 0, 0.0);
        public static readonly HitZoneInfo Head = new(HitZone.Head, 105, 7.0);
        public static readonly HitZoneInfo Body = new(HitZone.Body, 60, 5.5);
        public static readonly HitZoneInfo Limb = new(HitZone.Limb, 34, 4.0);
    }
}
