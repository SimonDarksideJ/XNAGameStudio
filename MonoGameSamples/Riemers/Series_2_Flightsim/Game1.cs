using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Series3D2
{
    public class Game1 : Game
    {
        public enum CollisionType { None, Building, Boundary, Target }

        public struct Bullet
        {
            public Vector3 position;
            public Quaternion rotation;
        }

        //Properties
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GraphicsDevice _device;
        private Effect _effect;

        private Matrix _viewMatrix;
        private Matrix _projectionMatrix;
        private Texture2D _sceneryTexture;
        private int[,] _floorPlan;
        private VertexBuffer _cityVertexBuffer;
        private int[] _buildingHeights = new int[] { 0, 2, 2, 6, 5, 4 };
        private Model _xwingModel;
        private Vector3 _lightDirection = new Vector3(3, -2, 5);
        private Vector3 _xwingPosition = new Vector3(8, 1, -3);
        private Quaternion _xwingRotation = Quaternion.Identity;
        private float _gameSpeed = 1.0f;
        private BoundingBox[] _buildingBoundingBoxes;
        private BoundingBox _completeCityBox;
        private Model _targetModel;
        private const int _maxTargets = 50;
        private List<Bullet> _bulletList = new List<Bullet>();
        private double _lastBulletTime = 0;
        private Texture2D _bulletTexture;

        private List<BoundingSphere> _targetList = new List<BoundingSphere>();
        private Vector3 _cameraPosition;
        private Vector3 _cameraUpDirection;
        private Texture2D[] _skyboxTextures;
        private Model _skyboxModel;
        private Quaternion _cameraRotation = Quaternion.Identity;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 500;
            _graphics.PreferredBackBufferHeight = 500;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();
            Window.Title = "Riemer's MonoGame Tutorials -- 3D Series 2";

            LoadFloorPlan();

            _lightDirection.Normalize();

            base.Initialize();
        }

        private void SetUpCamera()
        {
            _viewMatrix = Matrix.CreateLookAt(new Vector3(20, 13, -5), new Vector3(8, 0, -7), new Vector3(0, 1, 0));
            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, _device.Viewport.AspectRatio, 0.2f, 500.0f);
        }

        private void SetUpVertices()
        {
            int differentBuildings = _buildingHeights.Length - 1;
            float imagesInTexture = 1 + differentBuildings * 2;

            int cityWidth = _floorPlan.GetLength(0);
            int cityLength = _floorPlan.GetLength(1);

            List<VertexPositionNormalTexture> verticesList = new List<VertexPositionNormalTexture>();
            for (int x = 0; x < cityWidth; x++)
            {
                for (int z = 0; z < cityLength; z++)
                {
                    int currentBuilding = _floorPlan[x, z];

                    //floor or ceiling
                    verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, _buildingHeights[currentBuilding], -z), new Vector3(0, 1, 0), new Vector2(currentBuilding * 2 / imagesInTexture, 1)));
                    verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, _buildingHeights[currentBuilding], -z - 1), new Vector3(0, 1, 0), new Vector2((currentBuilding * 2) / imagesInTexture, 0)));
                    verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, _buildingHeights[currentBuilding], -z), new Vector3(0, 1, 0), new Vector2((currentBuilding * 2 + 1) / imagesInTexture, 1)));

                    verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, _buildingHeights[currentBuilding], -z - 1), new Vector3(0, 1, 0), new Vector2((currentBuilding * 2) / imagesInTexture, 0)));
                    verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, _buildingHeights[currentBuilding], -z - 1), new Vector3(0, 1, 0), new Vector2((currentBuilding * 2 + 1) / imagesInTexture, 0)));
                    verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, _buildingHeights[currentBuilding], -z), new Vector3(0, 1, 0), new Vector2((currentBuilding * 2 + 1) / imagesInTexture, 1)));

                    if (currentBuilding != 0)
                    {
                        //front wall
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z - 1), new Vector3(0, 0, -1), new Vector2((currentBuilding * 2) / imagesInTexture, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, _buildingHeights[currentBuilding], -z - 1), new Vector3(0, 0, -1), new Vector2((currentBuilding * 2 - 1) / imagesInTexture, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z - 1), new Vector3(0, 0, -1), new Vector2((currentBuilding * 2 - 1) / imagesInTexture, 1)));

                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, _buildingHeights[currentBuilding], -z - 1), new Vector3(0, 0, -1), new Vector2((currentBuilding * 2 - 1) / imagesInTexture, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z - 1), new Vector3(0, 0, -1), new Vector2((currentBuilding * 2) / imagesInTexture, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, _buildingHeights[currentBuilding], -z - 1), new Vector3(0, 0, -1), new Vector2((currentBuilding * 2) / imagesInTexture, 0)));

                        //back wall
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z), new Vector3(0, 0, 1), new Vector2((currentBuilding * 2) / imagesInTexture, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z), new Vector3(0, 0, 1), new Vector2((currentBuilding * 2 - 1) / imagesInTexture, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, _buildingHeights[currentBuilding], -z), new Vector3(0, 0, 1), new Vector2((currentBuilding * 2 - 1) / imagesInTexture, 0)));

                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, _buildingHeights[currentBuilding], -z), new Vector3(0, 0, 1), new Vector2((currentBuilding * 2 - 1) / imagesInTexture, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, _buildingHeights[currentBuilding], -z), new Vector3(0, 0, 1), new Vector2((currentBuilding * 2) / imagesInTexture, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z), new Vector3(0, 0, 1), new Vector2((currentBuilding * 2) / imagesInTexture, 1)));

                        //left wall
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z), new Vector3(-1, 0, 0), new Vector2((currentBuilding * 2) / imagesInTexture, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z - 1), new Vector3(-1, 0, 0), new Vector2((currentBuilding * 2 - 1) / imagesInTexture, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, _buildingHeights[currentBuilding], -z - 1), new Vector3(-1, 0, 0), new Vector2((currentBuilding * 2 - 1) / imagesInTexture, 0)));

                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, _buildingHeights[currentBuilding], -z - 1), new Vector3(-1, 0, 0), new Vector2((currentBuilding * 2 - 1) / imagesInTexture, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, _buildingHeights[currentBuilding], -z), new Vector3(-1, 0, 0), new Vector2((currentBuilding * 2) / imagesInTexture, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z), new Vector3(-1, 0, 0), new Vector2((currentBuilding * 2) / imagesInTexture, 1)));

                        //right wall
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z), new Vector3(1, 0, 0), new Vector2((currentBuilding * 2) / imagesInTexture, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, _buildingHeights[currentBuilding], -z - 1), new Vector3(1, 0, 0), new Vector2((currentBuilding * 2 - 1) / imagesInTexture, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z - 1), new Vector3(1, 0, 0), new Vector2((currentBuilding * 2 - 1) / imagesInTexture, 1)));

                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, _buildingHeights[currentBuilding], -z - 1), new Vector3(1, 0, 0), new Vector2((currentBuilding * 2 - 1) / imagesInTexture, 0)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z), new Vector3(1, 0, 0), new Vector2((currentBuilding * 2) / imagesInTexture, 1)));
                        verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, _buildingHeights[currentBuilding], -z), new Vector3(1, 0, 0), new Vector2((currentBuilding * 2) / imagesInTexture, 0)));
                    }
                }
            }

            _cityVertexBuffer = new VertexBuffer(_device, VertexPositionNormalTexture.VertexDeclaration, verticesList.Count, BufferUsage.WriteOnly);
            _cityVertexBuffer.SetData<VertexPositionNormalTexture>(verticesList.ToArray());
        }

        private void LoadFloorPlan()
        {
            _floorPlan = new int[,]
            {
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,1,1,0,0,0,1,1,0,0,1,0,1},
                {1,0,0,1,1,0,0,0,1,0,0,0,1,0,1},
                {1,0,0,0,1,1,0,1,1,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,1,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,1,1,0,0,0,1,0,0,0,0,0,0,1},
                {1,0,1,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,1,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,1,0,0,0,1,0,0,0,0,1},
                {1,0,1,0,0,0,0,0,0,1,0,0,0,0,1},
                {1,0,1,1,0,0,0,0,1,1,0,0,0,1,1},
                {1,0,0,0,0,0,0,0,1,1,0,0,0,1,1},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            };

            Random random = new Random();
            int differentBuildings = _buildingHeights.Length - 1;
            for (int x = 0; x < _floorPlan.GetLength(0); x++)
            {
                for (int y = 0; y < _floorPlan.GetLength(1); y++)
                {
                    if (_floorPlan[x, y] == 1)
                    {
                        _floorPlan[x, y] = random.Next(differentBuildings) + 1;
                    }
                }
            }
        }

        private Model LoadModel(string assetName)
        {
            Model newModel = Content.Load<Model>(assetName);
            foreach (ModelMesh mesh in newModel.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = _effect.Clone();
                }
            }
            return newModel;
        }

        private Model LoadModel(string assetName, out Texture2D[] textures)
        {
            Model newModel = Content.Load<Model>(assetName);
            List<Texture2D> modelTextures = new List<Texture2D>();

            foreach (ModelMesh mesh in newModel.Meshes)
            {
                foreach (BasicEffect currentEffect in mesh.Effects)
                {
                    modelTextures.Add(currentEffect.Texture);
                }

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = _effect.Clone();
                }
            }

            textures = modelTextures.ToArray();
            return newModel;
        }

        private void SetUpBoundingBoxes()
        {
            int cityWidth = _floorPlan.GetLength(0);
            int cityLength = _floorPlan.GetLength(1);

            List<BoundingBox> bbList = new List<BoundingBox>();
            for (int x = 0; x < cityWidth; x++)
            {
                for (int z = 0; z < cityLength; z++)
                {
                    int buildingType = _floorPlan[x, z];
                    if (buildingType != 0)
                    {
                        int buildingHeight = _buildingHeights[buildingType];
                        Vector3[] buildingPoints = new Vector3[2];
                        buildingPoints[0] = new Vector3(x, 0, -z);
                        buildingPoints[1] = new Vector3(x + 1, buildingHeight, -z - 1);
                        BoundingBox buildingBox = BoundingBox.CreateFromPoints(buildingPoints);
                        bbList.Add(buildingBox);
                    }
                }
            }
            _buildingBoundingBoxes = bbList.ToArray();

            Vector3[] boundaryPoints = new Vector3[2];
            boundaryPoints[0] = new Vector3(0, 0, 0);
            boundaryPoints[1] = new Vector3(cityWidth, 20, -cityLength);
            _completeCityBox = BoundingBox.CreateFromPoints(boundaryPoints);
        }

        private void AddTargets()
        {
            int cityWidth = _floorPlan.GetLength(0);
            int cityLength = _floorPlan.GetLength(1);

            Random random = new Random();

            while (_targetList.Count < _maxTargets)
            {
                int x = random.Next(cityWidth);
                int z = -random.Next(cityLength);
                float y = (float)random.Next(2000) / 1000f + 1;
                float radius = (float)random.Next(1000) / 1000f * 0.2f + 0.01f;

                BoundingSphere newTarget = new BoundingSphere(new Vector3(x, y, z), radius);

                if (CheckCollision(newTarget) == CollisionType.None)
                {
                    _targetList.Add(newTarget);
                }
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _device = _graphics.GraphicsDevice;
            _effect = Content.Load<Effect>("effects");
            _sceneryTexture = Content.Load<Texture2D>("texturemap");
            _bulletTexture = Content.Load<Texture2D>("bullet");

            _xwingModel = LoadModel("xwing");
            _targetModel = LoadModel("target");
            _skyboxModel = LoadModel("skybox", out _skyboxTextures);

            SetUpCamera();
            SetUpVertices();
            SetUpBoundingBoxes();
            AddTargets();
        }

        private void UpdateCamera()
        {
            _cameraRotation = Quaternion.Lerp(_cameraRotation, _xwingRotation, 0.1f);

            Vector3 cameraPosition = new Vector3(0, 0.1f, 0.6f);
            cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateFromQuaternion(_cameraRotation));
            cameraPosition += _xwingPosition;

            Vector3 cameraUpDirection = new Vector3(0, 1, 0);
            cameraUpDirection = Vector3.Transform(cameraUpDirection, Matrix.CreateFromQuaternion(_cameraRotation));

            _viewMatrix = Matrix.CreateLookAt(cameraPosition, _xwingPosition, cameraUpDirection);
            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, _device.Viewport.AspectRatio, 0.2f, 500.0f);

            _cameraPosition = cameraPosition;
            _cameraUpDirection = cameraUpDirection;
        }

        private void ProcessKeyboard(GameTime gameTime)
        {
            float leftRightRotation = 0;
            float upDownRotation = 0;

            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            turningSpeed *= 1.6f * _gameSpeed;

            KeyboardState keys = Keyboard.GetState();

            if (keys.IsKeyDown(Keys.Right))
            {
                leftRightRotation += turningSpeed;
            }
            if (keys.IsKeyDown(Keys.Left))
            {
                leftRightRotation -= turningSpeed;
            }
            if (keys.IsKeyDown(Keys.Down))
            {
                upDownRotation += turningSpeed;
            }
            if (keys.IsKeyDown(Keys.Up))
            {
                upDownRotation -= turningSpeed;
            }

            Quaternion additionalRotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), leftRightRotation) * Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), upDownRotation);
            _xwingRotation *= additionalRotation;

            if (keys.IsKeyDown(Keys.Space))
            {
                double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
                if (currentTime - _lastBulletTime > 100)
                {
                    Bullet newBullet = new Bullet();
                    newBullet.position = _xwingPosition;
                    newBullet.rotation = _xwingRotation;
                    _bulletList.Add(newBullet);

                    _lastBulletTime = currentTime;
                }
            }
        }

        private void MoveForward(ref Vector3 position, Quaternion rotationQuat, float speed)
        {
            Vector3 addVector = Vector3.Transform(new Vector3(0, 0, -1), rotationQuat);
            position += addVector * speed;
        }

        private CollisionType CheckCollision(BoundingSphere sphere)
        {
            for (int i = 0; i < _buildingBoundingBoxes.Length; i++)
            {
                if (_buildingBoundingBoxes[i].Contains(sphere) != ContainmentType.Disjoint)
                {
                    return CollisionType.Building;
                }
            }

            if (_completeCityBox.Contains(sphere) != ContainmentType.Contains)
            {
                return CollisionType.Boundary;
            }

            for (int i = 0; i < _targetList.Count; i++)
            {
                if (_targetList[i].Contains(sphere) != ContainmentType.Disjoint)
                {
                    _targetList.RemoveAt(i);
                    i--;
                    AddTargets();

                    return CollisionType.Target;
                }
            }

            return CollisionType.None;
        }

        private void UpdateBulletPositions(float moveSpeed)
        {
            for (int i = 0; i < _bulletList.Count; i++)
            {
                Bullet currentBullet = _bulletList[i];
                MoveForward(ref currentBullet.position, currentBullet.rotation, moveSpeed * 2.0f);
                _bulletList[i] = currentBullet;

                BoundingSphere bulletSphere = new BoundingSphere(currentBullet.position, 0.05f);
                CollisionType colType = CheckCollision(bulletSphere);
                if (colType != CollisionType.None)
                {
                    _bulletList.RemoveAt(i);
                    i--;

                    if (colType == CollisionType.Target)
                        _gameSpeed *= 1.05f;
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            UpdateCamera();
            ProcessKeyboard(gameTime);

            float moveSpeed = gameTime.ElapsedGameTime.Milliseconds / 500.0f * _gameSpeed;
            MoveForward(ref _xwingPosition, _xwingRotation, moveSpeed);

            BoundingSphere xwingSpere = new BoundingSphere(_xwingPosition, 0.04f);
            if (CheckCollision(xwingSpere) != CollisionType.None)
            {
                _xwingPosition = new Vector3(8, 1, -3);
                _xwingRotation = Quaternion.Identity;
                _gameSpeed /= 1.1f;
            }

            UpdateBulletPositions(moveSpeed);

            base.Update(gameTime);
        }

        private void DrawCity()
        {
            _effect.CurrentTechnique = _effect.Techniques["Textured"];
            _effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            _effect.Parameters["xView"].SetValue(_viewMatrix);
            _effect.Parameters["xProjection"].SetValue(_projectionMatrix);
            _effect.Parameters["xTexture"].SetValue(_sceneryTexture);
            _effect.Parameters["xEnableLighting"].SetValue(true);
            _effect.Parameters["xLightDirection"].SetValue(_lightDirection);
            _effect.Parameters["xAmbient"].SetValue(0.5f);

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _device.SetVertexBuffer(_cityVertexBuffer);
                _device.DrawPrimitives(PrimitiveType.TriangleList, 0, _cityVertexBuffer.VertexCount / 3);
            }
        }

        private void DrawModel()
        {
            Matrix worldMatrix = Matrix.CreateScale(0.0005f, 0.0005f, 0.0005f) *
                                 Matrix.CreateRotationY(MathHelper.Pi) *
                                 Matrix.CreateFromQuaternion(_xwingRotation) *
                                 Matrix.CreateTranslation(_xwingPosition);

            Matrix[] xwingTransforms = new Matrix[_xwingModel.Bones.Count];
            _xwingModel.CopyAbsoluteBoneTransformsTo(xwingTransforms);

            foreach (ModelMesh mesh in _xwingModel.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Colored"];
                    currentEffect.Parameters["xWorld"].SetValue(xwingTransforms[mesh.ParentBone.Index] * worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(_viewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(_projectionMatrix);
                    currentEffect.Parameters["xEnableLighting"].SetValue(true);
                    currentEffect.Parameters["xLightDirection"].SetValue(_lightDirection);
                    currentEffect.Parameters["xAmbient"].SetValue(0.5f);
                }
                mesh.Draw();
            }
        }

        private void DrawTargets()
        {
            for (int i = 0; i < _targetList.Count; i++)
            {
                Matrix worldMatrix = Matrix.CreateScale(_targetList[i].Radius) * Matrix.CreateTranslation(_targetList[i].Center);

                Matrix[] targetTransforms = new Matrix[_targetModel.Bones.Count];
                _targetModel.CopyAbsoluteBoneTransformsTo(targetTransforms);
                foreach (ModelMesh modelMesh in _targetModel.Meshes)
                {
                    foreach (Effect currentEffect in modelMesh.Effects)
                    {
                        currentEffect.CurrentTechnique = currentEffect.Techniques["Colored"];
                        currentEffect.Parameters["xWorld"].SetValue(targetTransforms[modelMesh.ParentBone.Index] * worldMatrix);
                        currentEffect.Parameters["xView"].SetValue(_viewMatrix);
                        currentEffect.Parameters["xProjection"].SetValue(_projectionMatrix);
                        currentEffect.Parameters["xEnableLighting"].SetValue(true);
                        currentEffect.Parameters["xLightDirection"].SetValue(_lightDirection);
                        currentEffect.Parameters["xAmbient"].SetValue(0.5f);
                    }

                    modelMesh.Draw();
                }
            }
        }

        private void DrawBullets()
        {
            if (_bulletList.Count > 0)
            {
                VertexPositionTexture[] bulletVertices = new VertexPositionTexture[_bulletList.Count * 6];
                int i = 0;

                foreach (Bullet currentBullet in _bulletList)
                {
                    Vector3 center = currentBullet.position;

                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 1));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 0));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 0));

                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 1));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 1));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 0));
                }

                _effect.CurrentTechnique = _effect.Techniques["PointSprites"];
                _effect.Parameters["xWorld"].SetValue(Matrix.Identity);
                _effect.Parameters["xProjection"].SetValue(_projectionMatrix);
                _effect.Parameters["xView"].SetValue(_viewMatrix);
                _effect.Parameters["xCamPos"].SetValue(_cameraPosition);
                _effect.Parameters["xTexture"].SetValue(_bulletTexture);
                _effect.Parameters["xCamUp"].SetValue(_cameraUpDirection);
                _effect.Parameters["xPointSpriteSize"].SetValue(0.1f);

                _device.BlendState = BlendState.Additive;

                foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _device.DrawUserPrimitives(PrimitiveType.TriangleList, bulletVertices, 0, _bulletList.Count * 2);
                }
                _device.BlendState = BlendState.Opaque;

            }

        }

        private void DrawSkybox()
        {
            SamplerState ss = new SamplerState();
            ss.AddressU = TextureAddressMode.Clamp;
            ss.AddressV = TextureAddressMode.Clamp;
            _device.SamplerStates[0] = ss;

            DepthStencilState dss = new DepthStencilState();
            dss.DepthBufferEnable = false;
            _device.DepthStencilState = dss;

            Matrix[] skyboxTransforms = new Matrix[_skyboxModel.Bones.Count];
            _skyboxModel.CopyAbsoluteBoneTransformsTo(skyboxTransforms);
            int i = 0;
            foreach (ModelMesh mesh in _skyboxModel.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = skyboxTransforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(_xwingPosition);
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(_viewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(_projectionMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(_skyboxTextures[i++]);
                }
                mesh.Draw();
            }

            dss = new DepthStencilState();
            dss.DepthBufferEnable = true;
            _device.DepthStencilState = dss;
        }
        protected override void Draw(GameTime gameTime)
        {
            _device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);

            DrawSkybox();
            DrawCity();
            DrawModel();
            DrawTargets();
            DrawBullets();

            base.Draw(gameTime);
        }
    }
}
