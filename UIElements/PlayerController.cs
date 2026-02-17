using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _2D_Engine_Sokov.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Input;
namespace _2D_Engine_Sokov.UIElements
{
    public class PlayerController:UIElement
    {
        public static Building placeBuilding = null;
        public static bool placingBuilding=false;
        private Vector2 firstSelection = Vector2.Zero;
        private Vector2 lastSelection = Vector2.Zero;
        public List<Unit> selectedUnits = new();

        public override void Update(double deltaTime)
        {

            if (Game.keyboardState.IsKeyDown(Keys.W))
            {
                var camera = RenderSystem.GetCamera();
                camera.Move(new Microsoft.Xna.Framework.Vector2(0, -1000 * (float)deltaTime)); // Движение вправо          
            }
            if (Game.keyboardState.IsKeyDown(Keys.S))
            {
                var camera = RenderSystem.GetCamera();
                camera.Move(new Microsoft.Xna.Framework.Vector2(0, 1000 * (float)deltaTime)); // Движение вправо      

            }
            if (Game.keyboardState.IsKeyDown(Keys.A))
            {
                var camera = RenderSystem.GetCamera();
                camera.Move(new Microsoft.Xna.Framework.Vector2(-1000 * (float)deltaTime, 0)); // Движение вправо                                                
            }
            if (Game.keyboardState.IsKeyDown(Keys.D))
            {
                var camera = RenderSystem.GetCamera();
                camera.Move(new Microsoft.Xna.Framework.Vector2(1000 * (float)deltaTime, 0)); // Движение вправо                                                
            }



            HandleSelection();

            //base.Update(deltaTime);
        }
        private void HandleSelection() {
            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();

            if (mouse.RightButton == ButtonState.Pressed)
            {
                var mousePos = new Vector2(mouse.X, mouse.Y);
                var camera = RenderSystem.GetCamera();
                var worldPos = Vector2.Transform(mousePos, Matrix.Invert(camera.TransformMatrix));
                //Vector2[] posUnits = new Vector2[selectedUnits.Count];
                //for (int i =0;i< selectedUnits.Count; i++)
                //{
                //    var unit = selectedUnits[i];
                //    posUnits[i] = unit.Position;
                //    unit.Path = Pathfinding.FindPath(Game.instance._currentLevel.TileMap, unit.Position, worldPos+(posUnits[i]- posUnits[0]));
                //}
                //Console.WriteLine(worldPos);
                if (placingBuilding) {
                    var build = Activator.CreateInstance(placeBuilding.GetType());
                    var building = ((Building)build);
                    building.Position = worldPos;
                    building.Size = placeBuilding.Size;
                    building.Texture = placeBuilding.Texture;
                    building.CollisionEnabled = placeBuilding.CollisionEnabled;
                    building.GravityEnabled = placeBuilding.GravityEnabled;
                    building.Mass = placeBuilding.Mass;
                    building.ProduceUnit = placeBuilding.ProduceUnit;
                    building.ProducingTime = placeBuilding.ProducingTime;
                    building.ProduceOffset = placeBuilding.ProduceOffset;
                    if (building != null)
                    {                        
                        Game.SubmitObject(building);
                    }
                    placingBuilding = false;
                }
                foreach (Unit unit in selectedUnits)
                    unit.Path = Pathfinding.FindPath(Game.instance._currentLevel.TileMap, unit.Position, worldPos + (unit.Position - selectedUnits.First().Position));
            

        }
            else if (mouse.LeftButton == ButtonState.Pressed)
            {
                
                var mousePos = new Vector2(mouse.X, mouse.Y);
                var camera = RenderSystem.GetCamera();
                var worldPos = Vector2.Transform(mousePos, Matrix.Invert(camera.TransformMatrix));
                if (firstSelection == Vector2.Zero) firstSelection = worldPos;
                else { 
                lastSelection = worldPos;
                int x = Math.Min((int)firstSelection.X, (int)lastSelection.X);
                int y = Math.Min((int)firstSelection.Y, (int)lastSelection.Y);
                int width = Math.Abs((int)(lastSelection.X - firstSelection.X));
                int height = Math.Abs((int)(lastSelection.Y - firstSelection.Y));
                Rectangle rectangle = new Rectangle(x, y, width, height);
                    //Rectangle rectangle = new Rectangle(firstSelection.ToPoint(), (lastSelection - firstSelection).ToPoint());
                RenderSystem.SubmitPersistentCommand(() => {
                    RenderSystem.DrawRectangle(rectangle, Color.Green, 3f);
                }, framesToLive: 3);
                }
                selectedUnits.Clear();
            }
            else if (mouse.LeftButton == ButtonState.Released)
            {               
            
                if ((firstSelection - lastSelection).LengthSquared() > 1) {
                    foreach (GameObject unit in LogicSystem.FindGameObjectsByTag("Player"))
                    {
                        if (unit is PlayerUnit)
                        {
                            var player = (PlayerUnit)unit;
                            int x = Math.Min((int)firstSelection.X, (int)lastSelection.X);
                            int y = Math.Min((int)firstSelection.Y, (int)lastSelection.Y);
                            int width = Math.Abs((int)(lastSelection.X - firstSelection.X));
                            int height = Math.Abs((int)(lastSelection.Y - firstSelection.Y));
                            Rectangle rectangle = new Rectangle(x, y, width, height);
                            //Rectangle rectangle = new Rectangle(firstSelection.ToPoint(), (lastSelection-firstSelection).ToPoint());
                             //RenderSystem.SubmitPersistentCommand(() => {
                              //              RenderSystem.DrawRectangle(rectangle, Color.Green, 3f);
                              //          }, framesToLive: 3);
                            if (rectangle.Contains(player.Position)) { 
                                selectedUnits.Add(player);
                            }
                        }

                    }
                    firstSelection = lastSelection = Vector2.Zero;
                }
            }
        }
    }
}
