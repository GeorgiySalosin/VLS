using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLSGame.Rendering
{
    public enum RenderOrder
    {
        Background = 0,      // Панорама,
        HUD = 1000          // Интерфейс (всегда сверху)
    }
}
