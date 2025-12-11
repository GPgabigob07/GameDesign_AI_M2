using System;
using Mechanics;
using UI.Controllers.Object_Details.Settings;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Controllers.Object_Details.Types
{
    public class MonkeyDetailsViewObject : IObjectDetailsViewObject
    {
        public Sprite Icon => null;
        public IDetailsCameraOptions CameraOptions { get; }

        public string Name => Monkey.Name;
        public MonkeyData Monkey { get; }

        public IContentDetails Content { get; }
        public string Description1 => $"{Monkey.Hp}♥";
        public string Description2 => $"{(Monkey.IsMale ? "Macho" : "Fêmea")}";
        public string Description3 => $"Atualmente está {Monkey.CurrentTask}";

        public MonkeyDetailsViewObject(MonkeyData monkey, VisualTreeAsset monkeyDetailsTemplate,
            VisualTreeAsset prowessTemplate)
        {
            Monkey = monkey;
            Content = new MonkeyProwessDetailsContent(monkeyDetailsTemplate.CloneTree()[0], prowessTemplate);
            CameraOptions = new MonkeyCameraOptions(monkey);
        }

        private class MonkeyProwessDetailsContent : IContentDetails
        {
            private readonly VisualElement _prowessContainer, _staminaBar, _root;
            private readonly VisualTreeAsset _prowessTemplate;
            private readonly MonkeyData _monkey;

            public MonkeyProwessDetailsContent(VisualElement root,
                VisualTreeAsset prowessTemplate)
            {
                _root = root;
                _prowessContainer = root.Q("monkeyDetailsRoot");
                _prowessTemplate = prowessTemplate;
                _staminaBar = root.Q("staminas");
            }

            public void Update(IObjectDetailsViewObject obj, GameUISettings settings, VisualElement content)
            {
                if (obj is not MonkeyDetailsViewObject vo) return;

                var monkey = vo.Monkey;
                var p = monkey.Prowess;
                _prowessContainer.Clear();
                foreach (var prowessTaskEntry in p)
                {
                    if (prowessTaskEntry.task == TaskType.Idle) continue;
                    
                    var element = _prowessTemplate.CloneTree()[0];
                    element.dataSource =
                        new ProwessUIViewObject(prowessTaskEntry, settings.GetProwessIcon(prowessTaskEntry.task));

                    element.RegisterCallback<ClickEvent>(_ =>
                    {
                        prowessTaskEntry.enabled = !prowessTaskEntry.enabled;
                        SoundEngine.PlaySFX(SoundEngine.bus.Sounds.uiClick, monkey.Self.transform);
                    });
                    _prowessContainer.Add(element);
                }
                
                _staminaBar.Clear();

                _root.dataSource = new MonkeyDetailsVODelegate(monkey, _staminaBar);
            }

            public VisualElement GetElement()
            {
                return _root;
            }
        }

        public void RemovePathPreview()
        {
            Monkey.Self.Agent.RemovePreview();
        }
    }

    public class ProwessUIViewObject
    {
        private readonly ProwessTaskEntry _prowessTask;

        [CreateProperty(ReadOnly = true)] public Sprite Icon { get; }

        [CreateProperty(ReadOnly = true)]
        public Color BackgroundColor
        {
            get
            {
                var color = _prowessTask.enabled
                    ? Color.white
                    : new Color(1f, .8f, .2f, .5f);
                return color;
            }
        }

        [CreateProperty(ReadOnly = true)] public string Points => $"{_prowessTask.points}";

        public ProwessUIViewObject(ProwessTaskEntry prowessTaskEntry, Sprite prowessIcon)
        {
            _prowessTask = prowessTaskEntry;
            Icon = prowessIcon;
        }
    }

    public class MonkeyCameraOptions : IDetailsCameraOptions
    {
        public Vector3 Position => _monkey.Self.transform.position;
        public float LensSize => 0.65f;
        private readonly MonkeyData _monkey;

        public MonkeyCameraOptions(MonkeyData monkey)
        {
            _monkey = monkey;
        }
    }

    public class MonkeyDetailsVODelegate
    {

        private static Sprite barBg;
        
        private int lastStamina = -1;
        private readonly MonkeyData _monkey;
        private readonly VisualElement _staminaBar;

        public MonkeyDetailsVODelegate(MonkeyData monkey, VisualElement sbar)
        {
            _monkey = monkey;
            _staminaBar = sbar;
            barBg ??= Resources.Load<Sprite>("UI/Sprites/HUD/status/gerenciamento/ger macacos/barrinha barra energia");
        }

        [CreateProperty]
        public int Stamina {
            get
            {
                var current = _monkey.ActionValue;
                if (lastStamina != current)
                    BuildStaminas();
                return current;   
            }
        }

        private void BuildStaminas()
        {
            lastStamina = _monkey.ActionValue;
            _staminaBar.Clear();
            for (var i = 0; i < lastStamina; i++)
            {
                var element = new VisualElement
                {
                    name = "staminaItem",
                    style =
                    {
                        height = 20,
                        backgroundImage = Background.FromSprite(barBg)
                    }
                };
                    
                element.AddToClassList("staminaItem");
                _staminaBar.Add(element);
            }
        }

    }
}