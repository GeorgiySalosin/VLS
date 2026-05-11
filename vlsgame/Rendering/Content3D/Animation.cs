using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLSGame.Rendering.Content3D
{
    /// <summary>
    /// additional class for animation management. Belongs to customobject3d (single instance per object)
    /// </summary>
    public class Animation 
    {

        public bool IsPlaying { get; set; } = false;

        private int currentFrame = 0;


        public int? CurrentFrame {
            get { return currentFrame; }
            set
            {
                if (value == null || value < 0 || value == int.MaxValue)
                {
                    IsPlaying = false;
                    currentFrame = 0;
                }
                else 
                    currentFrame = (int)value;
            } 
        }
    }
}
