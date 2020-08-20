#region File Description
//-----------------------------------------------------------------------------
// BasicEffectShape.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;
#endregion

namespace Spacewar
{
    /// <summary>
    /// The shapes that this class can draw
    /// </summary>
    public enum BasicEffectShapes
    {
        /// <summary>
        /// Ship models used ingame and in selection screen
        /// </summary>
        Ship,
        /// <summary>
        /// Asteroid models
        /// </summary>
        Asteroid,
        /// <summary>
        /// Projectile models
        /// </summary>
        Projectile,
        /// <summary>
        /// Weapon models for the selection screen
        /// </summary>
        Weapon,
    }

    /// <summary>
    /// BasicEffect shapes represent the 3d models used by the evolved variation of the game. These shapes come through the Content Pipeline.
    /// They are all lit using the BasicEffect Shader and have similar input and construction methods
    /// </summary>
    class BasicEffectShape : Shape
    {
        //Materials
        private static Color white = Color.White;
        private static Color body = white; //body has texture so don't need material
        private static Color pipes = white; //pipes has texture so don't need material
        private static Color cockpit1 = new Color((byte)(.529f * 255), 255, 255, 0);
        private static Color cockpit2 = new Color(255, 255, (byte)(.373f * 255), 0);
        private static Color engines = new Color((byte)(.925f * 255), (byte)(.529f * 255), 255, 0);

        private Texture2D texture;
        private int skinNumber;
        private int shapeNumber;
        private BasicEffectShapes shape = BasicEffectShapes.Projectile;
        private PlayerIndex player;

        private Model model; //New content Pipeline Mesh

        private bool deviceCreated;

        string[] modelNames = null;
        string[] textureNames = null;

        private int scene; //0 is in game 1 is selection screens

        private Color[] material;

        private static Vector3 SunPosition = new Vector3(-0.5f, -0.5f, 100.0f);

        #region filenames
        private static string[,] shipMesh = new string[,] 
        {
            {
                @"models\pencil_player1",
                @"models\saucer_player1",
                @"models\wedge_player1",
            },
            {
                @"models\pencil_player2",
                @"models\saucer_player2",
                @"models\wedge_player2",
            }
        };

        private static string[,] shipDiffuse = new string[,] 
        {
            {
                    @"textures\pencil_p1_diff_v{0}",
                    @"textures\saucer_p1_diff_v{0}",
                    @"textures\wedge_p1_diff_v{0}",
            },
            {
                    @"textures\pencil_p2_diff_v{0}",
                    @"textures\saucer_p2_diff_v{0}",
                    @"textures\wedge_p2_diff_v{0}",
            }
        };

        private static string[,] shipNormal = new String[,] 
        {
            {
                @"textures\pencil_p1_norm_1",
                @"textures\saucer_p1_norm_1",
                @"textures\wedge_p1_norm_1",
            },
            {
                @"textures\pencil_p2_norm_1",
                @"textures\saucer_p2_norm_1",
                @"textures\wedge_p2_norm_1",
            }
        };

        private static string[,] shipReflection = new String[,]
        {
            {
                @"textures\p1_reflection_cubemap",
                "",
            },
            {
                @"textures\p2_reflection_cubemap1",
                @"textures\p2_reflection_cubemap2",
            }
        };

        private static string[] projectileMeshes = new String[]
        {
            @"models\pea_proj",
            @"models\mgun_proj", 
            @"models\mgun_proj",
            @"models\p1_rocket_proj",
            @"models\bfg_proj",
        };

        private static string[] projectileDiffuse = new String[]
        {
            @"textures\pea_proj",
            @"textures\pea_proj",
            @"textures\mgun_proj",
            @"textures\rocket_proj",
            @"textures\bfg_proj",
        };

        private static string[] asteroidMeshes = new String[]
        {
            @"models\asteroid1",
            @"models\asteroid2",
        };

        private static string[] asteroidDiffuse = new String[]
        {
            @"textures\asteroid1",
            @"textures\asteroid2",
        };

        private static string[][] weaponMeshes = new String[][]
        {
            new string[] 
            {
                @"models\p1_pea",
                @"models\p1_mgun",
                @"models\p1_dual",
                @"models\p1_rocket",
                @"models\p1_bfg",
            },
            new string[]
            {
                @"models\p2_pea",
                @"models\p2_mgun",
                @"models\p2_dual",
                @"models\p2_rocket",
                @"models\p2_bfg",
            }
        };

        private static string[,][] weaponDiffuse = new String[,][]
        {
            {
                new string[] 
                {
                    @"textures\p1_back" // Used by p1_pea
                },
                new string[]
                {
                    @"textures\p1_back", @"textures\p1_dual" //Used by p1_mgun
                },
                new string[]
                {
                    @"textures\p1_dual", @"textures\p1_back" //Used by p1_dual
                },
                new string[]
                {
                    @"textures\p1_back", @"textures\p1_rocket" //Used by p1_rocket
                },
                new string[]
                {
                    @"textures\p1_bfg", "", @"textures\p1_back" //Used by the p1_bfg
                }
            },
            {
                new string[]
                {
                    @"textures\p2_back", @"textures\p2_back" // Used by p2_pea
                },
                new string[]
                {
                    @"textures\p2_back", @"textures\p2_back", @"textures\p2_dual" // Used by p2_mgun
                },
                new string[]
                {
                    @"textures\p2_back", @"textures\p2_back", @"textures\p2_dual" // Used by p2_dual
                },
                new string[]
                {
                    @"textures\p2_rocket", @"textures\p2_back", @"textures\p2_back" // Used by p2_rocket
                },
                new string[]
                {
                    @"textures\p2_back", @"textures\p2_back", @"textures\p2_bfg", @"textures\p2_back" // Used by p2_bfg
                }
            }
        };

        private Color[,][] shipMaterials = new Color[,][]
        {   
            {   //player 1 ships
                new Color[] {body, engines, cockpit1, cockpit1},
                new Color[] {body, cockpit1, engines},
                new Color[] {body, cockpit1, engines},
            },
            {   //player 2 ships
                new Color[] {cockpit2, pipes, body},
                new Color[] {pipes, body, cockpit2},
                new Color[] {pipes, cockpit2, body},
            }
        };

        private bool[,][] shipUsesReflection2 = new bool[,][]
        {   
            {   //player 1 ships - always use 1st reflection map
                new bool[] {false, false, false, false},
                new bool[] {false, false, false},
                new bool[] {false, false, false},
            },
            {   //player 2 ships
                new bool[] {true, true, false},
                new bool[] {true, false, true},
                new bool[] {true, true, false},
           }
        };

        #endregion

        public BasicEffectShape(Game game, BasicEffectShapes shape, PlayerIndex player, int shipNumber, int skinNumber, LightingType scene)
            : base(game)
        {
            Debug.Assert(shape == BasicEffectShapes.Ship, "Constructor should only be called with Ship");
            this.shape = shape;
            this.shapeNumber = shipNumber;
            this.skinNumber = skinNumber;
            this.player = player;
            this.scene = (int)scene;
            CreateShip();
        }

        public BasicEffectShape(Game game, BasicEffectShapes shape, PlayerIndex player, int shapeNumber, LightingType scene)
            : base(game)
        {
            Debug.Assert(shape == BasicEffectShapes.Weapon, "Constructor should only be called with Weapon");
            this.player = player;
            this.shape = shape;
            this.shapeNumber = shapeNumber;
            this.scene = (int)scene;
            CreateShape();
        }

        public BasicEffectShape(Game game, BasicEffectShapes shape, int shapeNumber, LightingType scene)
            : base(game)
        {
            this.shape = shape;
            this.shapeNumber = shapeNumber;
            this.scene = (int)scene;
            CreateShape();
        }

        public override void Create()
        {
            //Load the correct shader and set up the parameters
            OnCreateDevice();
        }

        public override void OnCreateDevice()
        {
            deviceCreated = true;
        }

        public void CreateShip()
        {
            //Model
            model = SpacewarGame.ContentManager.Load<Model>(SpacewarGame.Settings.MediaPath + shipMesh[(int)player, shapeNumber]);

            //Matching Textures
            texture = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + String.Format(shipDiffuse[(int)player, shapeNumber], (skinNumber + 1)));

            //Point to the right material array for this ship
            material = shipMaterials[(int)player, shapeNumber];

            SetupEffect();
        }

        public void CreateShape()
        {
            switch (shape)
            {
                case BasicEffectShapes.Projectile:
                    modelNames = projectileMeshes;
                    textureNames = projectileDiffuse;
                    break;

                case BasicEffectShapes.Asteroid:
                    modelNames = asteroidMeshes;
                    textureNames = asteroidDiffuse;
                    break;

                case BasicEffectShapes.Weapon:
                    modelNames = weaponMeshes[(int)player];
                    textureNames = weaponDiffuse[(int)player, shapeNumber];
                    break;

                default:
                    //Should never get here
                    Debug.Assert(true, "EvolvedShape:CreateShape - bad EvolvedShape passed in");
                    break;
            }

            //Model
            model = SpacewarGame.ContentManager.Load<Model>(SpacewarGame.Settings.MediaPath + modelNames[shapeNumber]);

            //Matching Textures
            if (shape == BasicEffectShapes.Asteroid || shape == BasicEffectShapes.Projectile)
                texture = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + textureNames[shapeNumber]);

            SetupEffect();
        }

        private void SetupEffect()
        {
            int i = 0;
            foreach (ModelMesh modelMesh in model.Meshes)
            {
                foreach (BasicEffect effect in modelMesh.Effects)
                {
                    //State
                    effect.Alpha = 1.0f;

                    effect.SpecularPower = 200.0f;
                    effect.AmbientLightColor = new Vector3(0.15f, 0.15f, 0.15f);

                    //Lighting
                    effect.LightingEnabled = true;

                    effect.DirectionalLight0.Enabled = true;
                    effect.DirectionalLight0.DiffuseColor =  new Vector3(SpacewarGame.Settings.ShipLights[scene].DirectionalColor.X,
                                                                        SpacewarGame.Settings.ShipLights[scene].DirectionalColor.Y,
                                                                        SpacewarGame.Settings.ShipLights[scene].DirectionalColor.Z);

                    effect.DirectionalLight0.SpecularColor = new Vector3(0.1f, 0.1f, 0.1f);

                    effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(SpacewarGame.Settings.ShipLights[scene].DirectionalDirection.X,
                                                                                        SpacewarGame.Settings.ShipLights[scene].DirectionalDirection.Y,
                                                                                        SpacewarGame.Settings.ShipLights[scene].DirectionalDirection.Z));

                    effect.DirectionalLight1.Enabled = true;
                    effect.DirectionalLight1.DiffuseColor = new Vector3(SpacewarGame.Settings.ShipLights[scene].PointColor.X,
                                                                         SpacewarGame.Settings.ShipLights[scene].PointColor.Y,
                                                                         SpacewarGame.Settings.ShipLights[scene].PointColor.Z);

                    effect.DirectionalLight1.SpecularColor = new Vector3(0.1f, 0.1f, 0.1f);

                    if (shape == BasicEffectShapes.Weapon)
                    {
                        if (string.IsNullOrEmpty(textureNames[i]))
                        {
                            texture = null;
                        }
                        else
                        {
                            texture = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + textureNames[i]);
                        }
                    }

                    effect.Texture = texture;
                    if (shape == BasicEffectShapes.Ship)
                    {
                        //Ships have materials and reflection maps
                        effect.SpecularColor =
                        effect.DiffuseColor = new Vector3(material[i].R / 255.0f, material[i].G / 255.0f, material[i].B / 255.0f);
                        if (material[i] == Color.White)
                        {
                            effect.TextureEnabled = true;
                        }
                        else
                        {
                            effect.TextureEnabled = false;
                        }
                    }
                    else
                    {
                        //Material is white
                        effect.SpecularColor =
                        effect.DiffuseColor = Vector3.One;
                        effect.TextureEnabled = true;
                    }
                }
            }
        }

        public override void Render()
        {
            base.Render();

            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));
            
            graphicsService.GraphicsDevice.BlendState = BlendState.Opaque;
            graphicsService.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            if (shape == BasicEffectShapes.Ship)
            {
                //Model
                model = SpacewarGame.ContentManager.Load<Model>(SpacewarGame.Settings.MediaPath + shipMesh[(int)player, shapeNumber]);

                //Matching Textures
                texture = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + String.Format(shipDiffuse[(int)player, shapeNumber], (skinNumber + 1)));
            }
            else
            {
                //Model
                model = SpacewarGame.ContentManager.Load<Model>(SpacewarGame.Settings.MediaPath + modelNames[shapeNumber]);

                //Matching Textures
                if (shape == BasicEffectShapes.Asteroid || shape == BasicEffectShapes.Projectile)
                    texture = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + textureNames[shapeNumber]);
            }

            if (deviceCreated)
            {
                SetupEffect();
                deviceCreated = false;
            }

            foreach (ModelMesh modelMesh in model.Meshes)
            {
                foreach (BasicEffect effect in modelMesh.Effects)
                {
                    //Transform
                    effect.View = SpacewarGame.Camera.View;
                    effect.Projection = SpacewarGame.Camera.Projection;
                    effect.World = World;
                    effect.Texture = texture;

                    // "fake" a point light by shining the directional light from the sun on the shape.
                    Vector3 direction = Position - SunPosition;
                    direction.Normalize();
                    effect.DirectionalLight1.Direction = direction;  
                }

                modelMesh.Draw();
            }
        }

        /// <summary>
        /// Preloads all the in game meshes and textures
        /// </summary>
        public static void Preload()
        {
        }
    }
}
