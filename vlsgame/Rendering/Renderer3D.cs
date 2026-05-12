using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VLSGame.Models;
using VLSGame.Rendering.Content2D.HUD;
using VLSGame.Rendering.Content3D;
using VLSGame.ViewModels;

namespace VLSGame.Rendering
{

    /// <summary>
    /// A class that stores all the 3d items and renders them to the viweport
    /// </summary>
    public sealed class Renderer3D
    {
        #region Initialization  
        public static Renderer3D Instance { get; } = new();
        private Renderer3D() { }

        private static bool isInitialized = false;

        public void Initialize(Viewport3D viewport)
        {
            if (isInitialized) return;

            this.viewport = viewport;

            isInitialized = true;
        }
        #endregion

        private Viewport3D viewport;

        private readonly static ObservableCollection<CustomObject3D> loadedObjects3D = [];     // a collection of custom 3d objects that participate in the scene rendering

        private readonly MatchTexturePool texturePool = MatchTexturePool.Instance;



        /// <summary>
        /// Adds a 3d object to the scene 3d dictionary
        /// </summary>
        public void AddObject(CustomObject3D obj) => loadedObjects3D.Add(obj);

        /// <summary>
        /// Gets a 3d object with specified id from the scene 3d dictionary
        /// </summary>
        public CustomObject3D GetObject(Guid id)
        {
            return loadedObjects3D.FirstOrDefault(x => x.Id == id);
        }
        /// <summary>
        /// Gets a 3d object with specified tag from the scene 3d dictionary
        /// </summary>
        public CustomObject3D GetObject(CustomObject3DTags tag)
        {
            return loadedObjects3D.FirstOrDefault(x => x.Tag == tag);
        }

        /// <summary>
        /// Removes a 3d object from the scene collection by corresponding guid
        /// </summary>

        public void RemoveObject(Guid id)
        {
            var obj = loadedObjects3D.FirstOrDefault(x => x.Id == id);
            if (obj != null)
            {
                //foreach (CustomObject3D child in obj.Children) RemoveObject(child.Id);      // recursively delete all the parented objects if any

                loadedObjects3D.Remove(obj);
                viewport.Children.Remove(obj.model);
            }
        }
        public void RemoveObject(CustomObject3D obj)
        {
            loadedObjects3D.Remove(obj);                            // remove from coll
            if (obj != null) viewport.Children.Remove(obj.model);   // unparent from viewport
        }

        /// <summary>
        /// Removes all 3d objects from the scene collection by corresponding tag 
        /// </summary>
        public void RemoveObject(string tag)
        {
            var objectsToRemove = loadedObjects3D.Where(obj => obj.Tag.Equals(tag)).ToList();
            if (objectsToRemove.Count > 0)
            foreach (var item in objectsToRemove)
            {
                loadedObjects3D.Remove(item);
                viewport.Children.Remove(item.model);
            }
        }

        /// <summary>
        /// Empties a scene 3d object collection
        /// </summary>
        public void RemoveAll()
        {
            loadedObjects3D.Clear();
            viewport.Children.Clear();
        }
            


        /// <summary>
        /// just reparents valid visible objects to the viewport, and unparents valid invisible
        /// </summary>
        public void Render()
        {
            foreach (var item in loadedObjects3D)
            {
                if (!viewport.Children.Contains(item.model) && item.IsVisible)
                {
                    viewport.Children.Add(item.model);

                }

                else if (viewport.Children.Contains(item.model) && !item.IsVisible)
                {
                    viewport.Children.Remove(item.model);
                }


                if (item.Animation.IsPlaying && item.Tag == CustomObject3DTags.FXAnimationSingle)
                {
                    int currentFrame = item.Animation.CurrentFrame ?? 0;

                    
                    if (currentFrame >= 20) 
                    {
                        RemoveObject(item);
                        break;
                    }
                    else
                    {
                        // Получаем текстуру для текущего кадра
                        var texture = texturePool.GetBloodFXTexture(ref currentFrame);
                        if (texture != null)
                            item.SetTexture(texture);

                        // Переходим к следующему кадру
                        item.Animation.CurrentFrame = currentFrame + 1;
                    }
                }
            }
        }





    }
}
