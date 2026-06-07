using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLSGame.Rendering
{
    /// <summary>
    /// additional class for animation management. Belongs to custom object 3d/2d (single instance per object)
    /// </summary>
    public class Animation (int framesCount = 0) 
    {
        /// <summary>determines if animation is playing or not</summary>
        public bool IsPlaying { get; private set; } = false;


        /// <summary>determines if animation should be played backwards or not</summary>
        public bool IsReversed { get; private set; } = false;


        /// <summary> the amount of animation frames </summary>
        public int FramesCount { get; private set; } = framesCount;


        private int currentFrame = 0;
        /// <summary>the current index of animation frames</summary>
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


        /// <summary>starts playing the animation from first to last frame</summary>
        public void PlayForward() { IsReversed = false; IsPlaying = true; }


        /// <summary>starts playing the animation from last to first frame</summary>
        public void PlayBackward() { IsReversed = true; IsPlaying = true; }


        /// <summary>stops the animation play</summary>
        public void Stop() { IsPlaying = false; }

    }
}
