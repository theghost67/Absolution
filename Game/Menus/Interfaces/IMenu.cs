﻿using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Game.Menus
{
    /// <summary>
    /// Интерфейс, реализующий объект как игровое меню.
    /// </summary>
    public interface IMenu
    {
        public event Action OnOpened;
        public event Action OnClosed;

        public GameObject GameObject { get; }
        public Transform Transform { get; }
        public string Id { get; }

        public bool IsDestroyed { get; }
        public bool IsOpened { get; }
        public bool ColliderEnabled { get; }
        public int SortingOrder { get; }
        public int OpenDepth { get; } // indicates menu index within opened ones (-1 when closed)
        public int FullDepth { get; } // indicates menu index within any other ones (-1 when destroyed)

        public Menu GetPrevious();
        public Menu GetPreviousOpened();

        public Menu GetNext();
        public Menu GetNextOpened();

        public UniTask DestroyAnimated();
        public UniTask ReturnAnimated();

        public UniTask OpenAnimated();
        public UniTask CloseAnimated();

        public void OpenInstantly();
        public void CloseInstantly();
        public void DestroyInstantly();

        public void WriteLog(string text);
        public void WriteDesc(string text);

        public void SetColliders(bool value);
        public void SetSortingOrder(int value);

    }
}
