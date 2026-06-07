using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLSGame.Models
{
    public enum ERifleState
    {
        Idle,           // Обычное состояние (можно стрелять, прицеливаться)
        ZoomingIn,      // Анимация приближения прицела (идёт вперёд)
        ZoomingOut,     // Анимация отдаления прицела (идёт назад)
        IdleZoom,       // Прицел приближён, можно стрелять
        Reloading       // Перезарядка (нельзя стрелять, прицеливаться, отменяет прицел)
    }
}
