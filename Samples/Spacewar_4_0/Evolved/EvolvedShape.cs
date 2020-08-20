#region File Description
//-----------------------------------------------------------------------------
// EvolvedShape.cs
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
    public enum EvolvedShapes
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
    /// Evolved shapes represents the 3d models used by the evolved variation of the game. They are all lit using the
    /// same shaders and have similar input and construction methods.
    /// </summary>
    class EvolvedShape : Shape
    {
        //Materials
        private static Color white = Color.White;
        private static Color body = white; //body has texture so don't need material
        private static Color pipes = white; //pipes has texture so don't need material
        private static Color cockpit1 = new Color((byte)(.529f * 255), 255, 255, 0);
        private static Color cockpit2 = new Color(255, 255, (byte)(.373f * 255), 0);
        private static Color engines = new Color((byte)(.925f * 255), (byte)(.529f * 255), 255, 0);

        private Texture2D texture;
        private TextureCube reflection1;
        private TextureCube reflection2;
        private int skinNumber;
        private int shapeNumber;
        private EvolvedShapes shape = EvolvedShapes.Projectile;
        private PlayerIndex player;

        private Model model; //New content Pipeline Mesh
        private Texture2D blackTexture;

        string[] modelNames = null;
        string[] textureNames = null;

        private int scene; //0 is in game 1 is selection screens

        private Color[] material;
        private bool[] useReflection2;

        private Effect effect;
        private EffectParameter worldParam;
        private EffectParameter inverseWorldParam;
        private EffectParameter worldViewProjectionParam;
        private EffectParameter viewPositionParam;
        private EffectParameter skinTextureParam;
        private EffectParameter normalTextureParam;
        private EffectParameter reflectionTextureParam;
        private EffectParameter ambientParam;
        private EffectParameter directionalDirectionParam;
        private EffectParameter directionalColorParam;
        private EffectParameter pointPositionParam;
        private EffectParameter pointColorParam;
        private EffectParameter pointFactorParam;
        private EffectParameter materialParam;

        #region filenames
        private static string[,] shipMesh = new string[,] 
        {
            {
                @"models\p1_pencil",
                @"models\p1_saucer",
                @"models\p1_wedge",
            },
            {
                @"models\p2_pencil",
                @"models\p2_saucer",
                @"models\p2_wedge",
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

        public EvolvedShape(Game game, EvolvedShapes shape, PlayerIndex player, int shipNumber, int skinNumber, LightingType scene)
            : base(game)
        {
            Debug.Assert(shape == EvolvedShapes.Ship, "Constructor should only be called with Ship");
            this.shape = shape;
            this.shapeNumber = shipNumber;
            this.skinNumber = skinNumber;
            this.player = player;
            this.scene = (int)scene;
            CreateShip();
        }

        public EvolvedShape(Game game, EvolvedShapes shape, PlayerIndex player, int shapeNumber, LightingType scene)
            : base(game)
        {
            Debug.Assert(shape == EvolvedShapes.Weapon, "Constructor should only be called with Weapon");
            this.player = player;
            this.shape = shape;
            this.shapeNumber = shapeNumber;
            this.scene = (int)scene;
            CreateShape();
        }

        public EvolvedShape(Game game, EvolvedShapes shape, int shapeNumber, LightingType scene)
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
            effect = SpacewarGame.ContentManager.Load<Effect>(SpacewarGame.Settings.MediaPath + @"shaders\ship");          

            worldParam = effect.Parameters["world"];
            inverseWorldParam = effect.Parameters["inverseWorld"];
            worldViewProjectionParam = effect.Parameters["worldViewProjection"];
            viewPositionParam = effect.Parameters["viewPosition"];
            skinTextureParam = effect.Parameters["SkinTexture"];
            normalTextureParam = effect.Parameters["NormalMapTexture"];
            reflectionTextureParam = effect.Parameters["ReflectionTexture"];
            ambientParam = effect.Parameters["Ambient"];
            directionalDirectionParam = effect.Parameters["DirectionalDirection"];
            directionalColorParam = effect.Parameters["DirectionalColor"];
            pointPositionParam = effect.Parameters["PointPosition"];
            pointColorParam = effect.Parameters["PointColor"];
            pointFactorParam = effect.Parameters["PointFactor"];
            materialParam = effect.Parameters["Material"];

            blackTexture = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"Textures\Black");

            SetupEffect();
        }

        private void SetupEffect()
        {
            ambientParam.SetValue(SpacewarGame.Settings.ShipLights[scene].Ambient);
            directionalDirectionParam.SetValue(SpacewarGame.Settings.ShipLights[scene].DirectionalDirection);
            directionalColorParam.SetValue(SpacewarGame.Settings.ShipLights[scene].DirectionalColor);
            pointPositionParam.SetValue(SpacewarGame.Settings.ShipLights[scene].PointPosition);
            pointColorParam.SetValue(SpacewarGame.Settings.ShipLights[scene].PointColor);
            pointFactorParam.SetValue(SpacewarGame.Settings.ShipLights[scene].PointFactor);

            normalTextureParam.SetValue((Texture2D)null); //Normal maps not currently used
        }

        public void CreateShip()
        {
            //Point to the right material array for this ship
            material = shipMaterials[(int)player, shapeNumber];

            //Point to the right cube map mapping
            useReflection2 = shipUsesReflection2[(int)player, shapeNumber];

            SetupEffect();
        }

        public void CreateShape()
        {
            switch (shape)
            {
                case EvolvedShapes.Projectile:
                    modelNames = projectileMeshes;
                    textureNames = projectileDiffuse;
                    break;

                case EvolvedShapes.Asteroid:
                    modelNames = asteroidMeshes;
                    textureNames = asteroidDiffuse;
                    break;

                case EvolvedShapes.Weapon:
                    modelNames = weaponMeshes[(int)player];
                    textureNames = weaponDiffuse[(int)player, shapeNumber];
                    break;

                default:
                    //Should never get here
                    Debug.Assert(true, "EvolvedShape:CreateShape - bad EvolvedShape passed in");
                    break;
            }

            SetupEffect();
        }

        public override void Render()
        {
            base.Render();            
            
            worldParam.SetValue(World);
            inverseWorldParam.SetValue(Matrix.Invert(World));
            worldViewProjectionParam.SetValue(World * SpacewarGame.Camera.View * SpacewarGame.Camera.Projection);
            viewPositionParam.SetValue(new Vector4(SpacewarGame.Camera.ViewPosition, 0));

            if (shape == EvolvedShapes.Ship)
            {
                //Model
                model = SpacewarGame.ContentManager.Load<Model>(SpacewarGame.Settings.MediaPath + shipMesh[(int)player, shapeNumber]);

                //Matching Textures
                texture = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + String.Format(shipDiffuse[(int)player, shapeNumber], (skinNumber + 1)));

                //_normal = SpacewarGame.TextureCache[shipNormal[(int)_player, _shapeNumber]]; //Normal maps not currently used

                reflection1 = SpacewarGame.ContentManager.Load<TextureCube>(SpacewarGame.Settings.MediaPath + shipReflection[(int)player, 0]);

                if (player == PlayerIndex.Two) //player 2 ship has 2 reflection maps
                {
                    reflection2 = SpacewarGame.ContentManager.Load<TextureCube>(SpacewarGame.Settings.MediaPath + shipReflection[(int)player, 1]);
                }
            }
            else
            {
                //Model
                
                model = SpacewarGame.ContentManager.Load<Model>(SpacewarGame.Settings.MediaPath + modelNames[shapeNumber]);

                //Matching Textures
                if (shape == EvolvedShapes.Asteroid || shape == EvolvedShapes.Projectile)
                    texture = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + textureNames[shapeNumber]);
            }

            int i = 0;
            foreach (ModelMesh modelMesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in modelMesh.MeshParts)
                {
                    if (shape == EvolvedShapes.Weapon)
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

                    skinTextureParam.SetValue(texture);

                    if (shape == EvolvedShapes.Ship)
                    {
                        //Ships have materials and reflection maps
                        materialParam.SetValue(material[i].ToVector4());
                        if (material[i] != Color.White)
                            skinTextureParam.SetValue(blackTexture);

                        //Choose the correct reflection map for this subset
                        reflectionTextureParam.SetValue(useReflection2[i] ? reflection2 : reflection1);
                    }
                    else
                    {
                        //Material is white
                        materialParam.SetValue(Color.White.ToVector4());
                        reflectionTextureParam.SetValue((Texture2D)null);
                    }

                    effect.Techniques[0].Passes[0].Apply();
                    
                    if (meshPart.PrimitiveCount > 0)
                    {
                        IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));
                        GraphicsDevice gd = graphicsService.GraphicsDevice;

                        gd.SetVertexBuffer(meshPart.VertexBuffer);
                        gd.Indices = meshPart.IndexBuffer;

                        gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshPart.VertexOffset, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                    }

                    i++;
                }
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
