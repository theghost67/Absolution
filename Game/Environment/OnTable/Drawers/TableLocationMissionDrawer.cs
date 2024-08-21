﻿using GreenOne;
using TMPro;
using UnityEngine;

namespace Game.Environment
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="TableLocation"/>.
    /// </summary>
    public sealed class TableLocationMissionDrawer : Drawer
    {
        static readonly GameObject _prefab;
        public readonly new TableLocationMission attached;
        readonly LocationMission _attachedData;
        readonly Drawer _buttonDrawer;

        static TableLocationMissionDrawer()
        {
            _prefab = Resources.Load<GameObject>("Prefabs/Mission");
        }
        public TableLocationMissionDrawer(TableLocationMission mission, Transform parent) : base(mission, _prefab, parent)
        {
            attached = mission;
            _attachedData = mission.Data;

            _buttonDrawer = new Drawer(null, transform.Find("Left square")).WithHoverScaleEvents();
            _buttonDrawer.OnMouseClick += (s, e) => attached.TryStartTravel();

            LocationEvent attachedEvent = _attachedData.@event;
            transform.Find<TextMeshPro>("Number").text = "II";
            transform.Find<TextMeshPro>("Header").text = $"Событие: {attachedEvent.name}";
            transform.Find<TextMeshPro>("Info").text = attachedEvent.desc;
            transform.Find<TextMeshPro>("Duration").text = $"Ур. длительности: {_attachedData.durationLevel.richName} ({_attachedData.durationLevel.value} кл.)";
            transform.Find<TextMeshPro>("Threat").text = $"Ур. угрозы: {_attachedData.threatLevel.richName} ({_attachedData.location.stage} ед.)";
        }

        protected override void SetCollider(bool value)
        {
            _buttonDrawer.ColliderEnabled = value;
        }
        protected override void SetSortingOrder(int value)
        {
            _buttonDrawer.SortingOrder = value;
        }
    }
}
