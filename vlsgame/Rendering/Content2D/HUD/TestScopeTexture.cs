using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLSGame.Rendering.Content2D.HUD
{
    public class TestScopeTexture : Texture
    {
        public TestScopeTexture() : base("Scope")
        {
            LoadFromFile("pack://application:,,,/Content/Animation/Rifle/Test_Debug_Scope.png");
        }
    }
}
