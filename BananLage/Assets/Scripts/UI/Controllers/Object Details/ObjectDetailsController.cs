using Mechanics;
using Structures;
using UI.Controllers.Object_Details.Settings;
using UI.Controllers.Object_Details.Types;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace UI.Controllers.Object_Details
{
    public class ObjectDetailsController : UIFragment
    {
        protected override string RootId => "detailsRoot";

        [SerializeField] private RenderTexture detailsTexture;
        [SerializeField] private Camera detailsCamera;
        [SerializeField] private float lensSizePerUnit = 0.65f;
        [SerializeField] private GameUISettings uiSettings;

        private RenderTexture _cameraOutput;
        private VisualElement _detailsContent, _vines;
        private IObjectDetailsViewObject _viewObject;

        protected override void OnCreateView()
        {
            _detailsContent = RootElement.Q<VisualElement>("detailsContentContainer");
            _vines = RootElement.Q<VisualElement>("vines");

            Assert.IsNotNull(_detailsContent);

            _cameraOutput = new RenderTexture(detailsTexture);
            _cameraOutput.Create();
            detailsCamera.targetTexture = _cameraOutput;
            SetObject(null);
        }

        private void Update()
        {
            if (_viewObject is null) return;
            var cam = _viewObject.CameraOptions;

            if (cam is null)
            {
                detailsCamera.transform.position = Vector3.zero;
                return;
            }

            detailsCamera.transform.position = cam.Position + new Vector3(0, 0, -10f);
            detailsCamera.orthographicSize = cam.LensSize;
        }

        public void SetObject(IObjectDetailsViewObject viewObject)
        {
            if (viewObject == null)
            {
                RootElement.style.display = DisplayStyle.None;

                if (_viewObject is MonkeyDetailsViewObject mvo)
                    mvo.RemovePathPreview();

                _viewObject = null;
                return;
            }

            RootElement.style.display = DisplayStyle.Flex;

            if (_viewObject == viewObject)
            {
                viewObject.Content.Update(viewObject, uiSettings, _detailsContent[0]);
                return;
            }

            if (_viewObject != null && _viewObject.GetType().IsAssignableFrom(viewObject.GetType()))
            {
                if (_viewObject is MonkeyDetailsViewObject mvo)
                    mvo.RemovePathPreview();
            }
            
            var element = viewObject.Content.GetElement();
            if (element is null)
            {
                _vines.visible = false;
                return;
            }
            
            _vines.visible = true;
            
            _detailsContent.Clear();
            _detailsContent.Add(element);

            _viewObject = viewObject;
            viewObject.Content.Update(viewObject, uiSettings, _detailsContent[0]);
            SetNameAndImage();

            _cameraOutput?.DiscardContents();
        }

        private void SetNameAndImage()
        {
            RootElement.dataSource = new ObjectDetailsViewObjectDelegate(_viewObject, _cameraOutput);
        }

        public void ConfigureMonkey(MonkeyData monkey) =>
            SetObject(new MonkeyDetailsViewObject(monkey, uiSettings.monkeyTemplate, uiSettings.prowessTemplate));

        public void ConfigureStructure(BaseStructure str)
        {
            SetObject(new StructureDetailsViewObject(str, uiSettings, false));
        }
        
        public void ConfigureBuild(StructureData structure) =>
            SetObject(new BuildingInfoViewObject(structure, uiSettings));
        
    }

    public class ObjectDetailsViewObjectDelegate : IObjectDetailsViewObject
    {
        private readonly IObjectDetailsViewObject delegated;

        public ObjectDetailsViewObjectDelegate(IObjectDetailsViewObject delegated, RenderTexture original)
        {
            this.delegated = delegated;
            CameraBackground = this.delegated.CameraOptions is null
                ? new StyleBackground(this.delegated.Icon)
                : new StyleBackground(Background.FromRenderTexture(original));
        }

        [CreateProperty] public string Name => delegated.Name;

        [CreateProperty]
        public Sprite Icon => delegated.Icon;

        public IDetailsCameraOptions CameraOptions => delegated.CameraOptions;

        public IContentDetails Content => delegated.Content;

        [CreateProperty]
        public string Description1 => delegated.Description1;

        [CreateProperty]
        public string Description2 => delegated.Description2;

        [CreateProperty]
        public string Description3 => delegated.Description3;
        
        [CreateProperty]
        public StyleBackground CameraBackground { get; }
    }
}