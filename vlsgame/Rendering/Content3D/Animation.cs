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
        public bool IsPlaying { get; private set; } = false;
        public bool IsReversed { get; private set; } = false;

        private int currentFrame = 0;
        public int? CurrentFrame
        {
            get => currentFrame;
            set
            {
                if (value == null || value < 0)
                {
                    IsPlaying = false;
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = value.Value;
                }
            }
        }

        public void PlayForward() { IsReversed = false; IsPlaying = true; }
        public void PlayBackward() { IsReversed = true; IsPlaying = true; }
        public void Stop() { IsPlaying = false; }
        public void Reset() { currentFrame = 0; IsPlaying = false; IsReversed = false; }
    }
}
