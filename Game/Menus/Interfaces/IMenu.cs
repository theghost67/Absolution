using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Game.Menus
{
    /// <summary>
    /// Интерфейс, реализующий объект как игровое меню.
    /// </summary>
    public interface IMenu : ITableEntrySource
    {
        public event Action OnOpened;
        public event Action OnClosed;

        public string Id { get; }
        public GameObject GameObject { get; }
        public Transform Transform { get; }
        public Func<Menu> MenuWhenClosed { get; set; }

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

        public UniTask TransitToThis();
        public UniTask TransitFromThis();

        // use 'from' to determine either this menu from which transition begins to 'to' menu
        public void OnTransitStart(bool from);  // invokes before transit animation
        public void OnTransitMiddle(bool from); // invokes at the black screen of transit animation
        public void OnTransitEnd(bool from);    // invokes after transit animation

        public void Open();
        public void Close();
        public void Destroy();

        public void WriteLog(string text);
        public void WriteDesc(string text);

        public void SetColliders(bool value);
        public void SetSortingOrder(int value);

    }
}
