using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLSGame.Models;

namespace VLSGame.ViewModels
{
    public sealed class RifleState : ViewModelBase
    {
        private ERifleState state = ERifleState.Idle;
        public ERifleState State
        {
            get => state;
            set => Set(ref state, value);
        }

        private bool hasAmmo = true;
        public bool HasAmmo
        {
            get => hasAmmo;
            set => Set(ref hasAmmo, value);
        }
    }
}
