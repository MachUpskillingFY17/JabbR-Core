﻿using System;
using System.Collections.Generic;

namespace JabbR_Core.ViewModels
{
    public class LobbyRoomViewModel
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public bool Private { get; set; }
        public bool Closed { get; set; }
        public string Topic { get; set; }
    }
}