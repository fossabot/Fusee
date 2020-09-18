using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Scene;
using Fusee.Engine.GUI;
using Fusee.Math.Core;
using Fusee.Pointcloud.Common;
using Fusee.Pointcloud.OoCFileReaderWriter;
using Fusee.Pointcloud.PointAccessorCollections;
using Fusee.Xene;
using System.Collections.Generic;
using System.Linq;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;

namespace Fusee.Examples.PcRendering.Core
{
    [FuseeApplication(Name = "FUSEE Point Cloud Viewer")]
    public class PcRendering<TPoint> : RenderCanvas, IPcRendering where TPoint : new()
    {
        public AppSetupHelper.AppSetupDelegate AppSetup;

        public PtOctantLoader<TPoint> OocLoader { get; set; }

        public PtOctreeFileReader<TPoint> OocFileReader { get; set; }

        public bool UseExtUi { get; set; }
        public bool DoShowOctants { get; set; }
        public bool IsSceneLoaded { get; private set; }
        public bool ReadyToLoadNewFile { get; private set; }
        public bool IsInitialized { get; private set; } = false;
        public bool IsAlive { get; private set; }
        public bool IsRenderPauseRequested { get; set; }
        public bool IsClosingRequested { get; set; }

        // angle variables
        private static float _angleHorz = 0, _angleVert = 0, _angleVelHorz, _angleVelVert, _angleRoll, _angleRollInit;
        private static float2 _offset;
        private static float2 _offsetInit;

        private const float RotationSpeed = 7;

        private SceneContainer _scene;
        private SceneRendererForward _sceneRenderer;

        private bool _twoTouchRepeated;
        private bool _keys;

        private const float ZNear = 1f;
        private const float ZFar = 1000;

        private readonly float _fovy = M.PiOver4;

        private float _maxPinchSpeed;

        private float3 _initCamPos;
        public float3 InitCameraPos { get { return _initCamPos; } private set { _initCamPos = value; OocLoader.InitCamPos = _initCamPos; } }

        private bool _isTexInitialized = false;

        private Texture _octreeTex;
        private double3 _octreeRootCenter;
        private double _octreeRootLength;

        private WritableTexture _depthTex;

        private Transform _camTransform;
        private Camera _cam;

        // Init is called on startup. 
        public override void Init()
        {
            _depthTex = new WritableTexture(RenderTargetTextureTypes.Depth, new ImagePixelFormat(ColorFormat.Depth16), Width, Height, false);

            IsAlive = true;
            AppSetup();

            _scene = new SceneContainer
            {
                Children = new List<SceneNode>()
            };

            _camTransform = new Transform()
            {
                Name = "MainCamTransform",
                Scale = float3.One,
                Translation = InitCameraPos,
                Rotation = float3.Zero
            };

            _cam = new Camera(ProjectionMethod.Perspective, ZNear, ZFar, _fovy)
            {
                BackgroundColor = float4.One
            };

            var mainCam = new SceneNode()
            {
                Name = "MainCam",
                Components = new List<SceneComponent>()
                {
                    _camTransform,
                    _cam
                }
            };

            _scene.Children.Insert(0, mainCam);

            _angleRoll = 0;
            _angleRollInit = 0;
            _twoTouchRepeated = false;
            _offset = float2.Zero;
            _offsetInit = float2.Zero;

            // Set the clear color for the back buffer to white (100% intensity in all color channels R, G, B, A).            

            if (!UseExtUi)
                LoadPointCloudFromFile();

            // Wrap a SceneRenderer around the model.
            _sceneRenderer = new SceneRendererForward(_scene);
            
            IsInitialized = true;
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            ReadyToLoadNewFile = false;

            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            if (IsRenderPauseRequested)
                return;

            if (IsSceneLoaded)
            {
                if (Keyboard.WSAxis != 0 || Keyboard.ADAxis != 0 || (Touch.GetTouchActive(TouchPoints.Touchpoint_0) && !Touch.TwoPoint))
                    OocLoader.IsUserMoving = true;
                else
                    OocLoader.IsUserMoving = false;

                // Mouse and keyboard movement
                if (Keyboard.LeftRightAxis != 0 || Keyboard.UpDownAxis != 0)
                {
                    _keys = true;
                }

                // Zoom & Roll
                if (Touch.TwoPoint)
                {
                    if (!_twoTouchRepeated)
                    {
                        _twoTouchRepeated = true;
                        _angleRollInit = Touch.TwoPointAngle - _angleRoll;
                        _offsetInit = Touch.TwoPointMidPoint - _offset;
                        _maxPinchSpeed = 0;
                    }

                    _angleRoll = Touch.TwoPointAngle - _angleRollInit;
                    _offset = Touch.TwoPointMidPoint - _offsetInit;
                    float pinchSpeed = Touch.TwoPointDistanceVel;
                    if (pinchSpeed > _maxPinchSpeed) _maxPinchSpeed = pinchSpeed; // _maxPinchSpeed is used for debugging only.
                }
                else
                {
                    _twoTouchRepeated = false;
                }

                // UpDown / LeftRight rotation
                if (Mouse.LeftButton)
                {
                    _keys = false;

                    _angleVelHorz = RotationSpeed * Mouse.XVel * DeltaTime * 0.0005f;
                    _angleVelVert = RotationSpeed * Mouse.YVel * DeltaTime * 0.0005f;
                }
                else if (Touch.GetTouchActive(TouchPoints.Touchpoint_0) && !Touch.TwoPoint)
                {
                    _keys = false;
                    float2 touchVel;
                    touchVel = Touch.GetVelocity(TouchPoints.Touchpoint_0);
                    _angleVelHorz = RotationSpeed * touchVel.x * DeltaTime * 0.0005f;
                    _angleVelVert = RotationSpeed * touchVel.y * DeltaTime * 0.0005f;
                }
                else
                {
                    if (_keys)
                    {
                        _angleVelHorz = RotationSpeed * Keyboard.LeftRightAxis * DeltaTime;
                        _angleVelVert = RotationSpeed * Keyboard.UpDownAxis * DeltaTime;
                    }
                }

                _angleHorz += _angleVelHorz;
                _angleVert += _angleVelVert;
                _angleVelHorz = 0;
                _angleVelVert = 0;

                if (HasUserMoved() || _camTransform.Translation == InitCameraPos)
                {
                    _camTransform.FpsView(_angleHorz, _angleVert, Keyboard.WSAxis, Keyboard.ADAxis, DeltaTime * 20);
                }

                //----------------------------  

                if (PtRenderingParams.CalcSSAO || PtRenderingParams.Lighting != Lighting.Unlit)
                {
                    //Render Depth-only pass                    
                    _scene.Children[1].RemoveComponent<ShaderEffect>();
                    _scene.Children[1].Components.Insert(1, PtRenderingParams.DepthPassEf);

                    _cam.RenderTexture = _depthTex;
                    _sceneRenderer.Render(RC);
                    _cam.RenderTexture = null;
                }

                //Render color pass
                //Change shader effect in complete scene
                _scene.Children[1].RemoveComponent<ShaderEffect>();
                _scene.Children[1].Components.Insert(1, PtRenderingParams.ColorPassEf);
                _sceneRenderer.Render(RC);

                //UpdateScene after Render / Traverse because there we calculate the view matrix (when using a camera) we need for the update.
                OocLoader.RC = RC;
                OocLoader.UpdateScene(PtRenderingParams.PtMode, PtRenderingParams.DepthPassEf, PtRenderingParams.ColorPassEf);

                if (UseExtUi)
                {
                    if (PtRenderingParams.ShaderParamsToUpdate.Count != 0)
                    {
                        UpdateShaderParams();
                        PtRenderingParams.ShaderParamsToUpdate.Clear();
                    }
                }

                if (DoShowOctants)
                    OocLoader.ShowOctants(_scene);
            }

            //Render GUI
            RC.Projection = float4x4.CreateOrthographic(Width, Height, ZNear, ZFar);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();

            ReadyToLoadNewFile = true;

            if (IsClosingRequested)
                CloseGameWindow();
        }

        // Is called when the window was resized
        public override void Resize(ResizeEventArgs e)
        {
            if (!PtRenderingParams.CalcSSAO && PtRenderingParams.Lighting == Lighting.Unlit) return;

            //(re)create depth tex and fbo
            if (_isTexInitialized)
            {
                _depthTex = new WritableTexture(RenderTargetTextureTypes.Depth, new ImagePixelFormat(ColorFormat.Depth16), Width, Height, false);

                PtRenderingParams.DepthPassEf.SetEffectParam("ScreenParams", new float2(Width, Height));
                PtRenderingParams.ColorPassEf.SetEffectParam("ScreenParams", new float2(Width, Height));
                PtRenderingParams.ColorPassEf.SetEffectParam("DepthTex", _depthTex);
            }

            _isTexInitialized = true;
        }

        public override void DeInit()
        {
            IsAlive = false;
            base.DeInit();

        }

        private bool HasUserMoved()
        {
            return RC.View == float4x4.Identity
                || Mouse.LeftButton
                || Keyboard.WSAxis != 0 || Keyboard.ADAxis != 0
                || (Touch.GetTouchActive(TouchPoints.Touchpoint_0) && !Touch.TwoPoint);
        }

        public RenderContext GetRc()
        {
            return RC;
        }

        public SceneNode GetOocLoaderRootNode()
        {
            return OocLoader.RootNode;
        }

        public bool GetOocLoaderWasSceneUpdated()
        {
            return OocLoader.WasSceneUpdated;
        }

        public int GetOocLoaderPointThreshold()
        {
            return OocLoader.PointThreshold;
        }

        public void SetOocLoaderPointThreshold(int value)
        {
            OocLoader.PointThreshold = value;
        }

        public void SetOocLoaderMinProjSizeMod(float value)
        {
            OocLoader.MinProjSizeModifier = value;
        }

        public float GetOocLoaderMinProjSizeMod()
        {
            return OocLoader.MinProjSizeModifier;
        }

        public void LoadPointCloudFromFile()
        {
            //create Scene from octree structure
            var root = OocFileReader.GetScene(ShaderCodeBuilder.Default/*PtRenderingParams.DepthPassEf*/);

            var ptOctantComp = root.GetComponent<Octant>();
            InitCameraPos = _camTransform.Translation = new float3((float)ptOctantComp.PayloadOctant.Center.x, (float)ptOctantComp.PayloadOctant.Center.y, (float)(ptOctantComp.PayloadOctant.Center.z - (ptOctantComp.PayloadOctant.Size)));

            _scene.Children.Add(root);

            OocLoader.RootNode = root;
            OocLoader.FileFolderPath = PtRenderingParams.PathToOocFile;

            var octreeTexImgData = new ImageData(ColorFormat.uiRgb8, OocFileReader.NumberOfOctants, 1);
            _octreeTex = new Texture(octreeTexImgData);
            OocLoader.VisibleOctreeHierarchyTex = _octreeTex;

            var byteSize = OocFileReader.NumberOfOctants * octreeTexImgData.PixelFormat.BytesPerPixel;
            octreeTexImgData.PixelData = new byte[byteSize];

            var ptRootComponent = root.GetComponent<Octant>();
            _octreeRootCenter = ptRootComponent.PayloadOctant.Center;
            _octreeRootLength = ptRootComponent.PayloadOctant.Size;

            PtRenderingParams.DepthPassEf = PtRenderingParams.CreateDepthPassEffect(new float2(Width, Height), InitCameraPos.z, _octreeTex, _octreeRootCenter, _octreeRootLength);
            PtRenderingParams.ColorPassEf = PtRenderingParams.CreateColorPassEffect(new float2(Width, Height), InitCameraPos.z, new float2(ZNear, ZFar), _depthTex, _octreeTex, _octreeRootCenter, _octreeRootLength);
            root.RemoveComponent<ShaderEffect>();
            root.Components.Insert(1, PtRenderingParams.DepthPassEf);

            if (PtRenderingParams.CalcSSAO || PtRenderingParams.Lighting != Lighting.Unlit)
            {
                _scene.Children[1].RemoveComponent<ShaderEffect>();
                _scene.Children[1].AddComponent(PtRenderingParams.DepthPassEf);
            }
            else
            {
                _scene.Children[1].RemoveComponent<ShaderEffect>();
                _scene.Children[1].AddComponent(PtRenderingParams.ColorPassEf);
            }

            IsSceneLoaded = true;
        }

        public void DeletePointCloud()
        {
            IsSceneLoaded = false;

            while (!OocLoader.WasSceneUpdated || !ReadyToLoadNewFile)
            {
                continue;
            }

            if (OocLoader.RootNode != null)
                _scene.Children.Remove(OocLoader.RootNode);

        }

        public void ResetCamera()
        {
            _camTransform.Translation = InitCameraPos;
            _angleHorz = _angleVert = 0;
        }

        public void DeleteOctants()
        {
            IsSceneLoaded = false;

            while (!OocLoader.WasSceneUpdated || !ReadyToLoadNewFile)
            {
                continue;
            }

            DoShowOctants = false;
            OocLoader.DeleteOctants(_scene);
            IsSceneLoaded = true;
        }

        private void UpdateShaderParams()
        {
            foreach (var param in PtRenderingParams.ShaderParamsToUpdate)
            {
                PtRenderingParams.DepthPassEf.SetEffectParam(param.Key, param.Value);
                PtRenderingParams.ColorPassEf.SetEffectParam(param.Key, param.Value);
            }

            PtRenderingParams.ShaderParamsToUpdate.Clear();
        }

    }
}