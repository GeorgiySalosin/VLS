using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLSGame.Rendering.Content3D
{
    public enum CustomObject3DTags
    {
        /// <summary> Default tag </summary>
        None,
        /// <summary> An object that represents panorama sphere</summary>
        World,
        /// <summary> An ambient light. Does not contain 3d model </summary>
        AmbientLight,
        /// <summary> An object that represents enemy spawned away</summary>
        Enemy,
        /// <summary> An object that represents bullet</summary>
        Projectile,
        /// <summary> An object that represents special effect that is played once (e.x. blood split on player hit)</summary>
        FXAnimationSingle
    }
}
