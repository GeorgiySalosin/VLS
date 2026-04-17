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
using VLSGame.Rendering.Content2D.HUD;
using VLSGame.Rendering.Content3D;
using VLSGame.ViewModels;

namespace VLSGame.Rendering
{

    /// <summary>
    /// A class that stores all the 3d items and renders them to the viweport
    /// </summary>
    public sealed class Renderer3D: ViewModelBase, INotifyCollectionChanged
    {
        #region Initialization  
        public static Renderer3D Instance { get; } = new();
        private Renderer3D() { }

        private static bool isInitialized = false;

        public void Initialize(Viewport3D viewport)
        {
            if (isInitialized) return;

            this.viewport = viewport;

            Subscribe();
            isInitialized = true;
        }
        #endregion

        private Viewport3D viewport;

        private readonly static ObservableCollection<CustomObject3D> loadedObjects3D = [];     // a collection of custom 3d objects that participate in the scene rendering




        #region Event Handlers  
        private void Subscribe()
        {
            PropertyChanged += OnPropertyChanged;
            loadedObjects3D.CollectionChanged += OnCollectionChanged;

        }

        private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == nameof(CustomObject3D.IsVisible))   // here we autoupdate a MESH if texture was changed.
            //{
            //    RefreshViewport();
            //}
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // УДАЛИТЬ ИЛИ ЗАКОММЕНТИРОВАТЬ RefreshViewport()
            // RefreshViewport();

            if (e.NewItems != null)
            {
                foreach (CustomObject3D obj in e.NewItems)
                {
                    obj.PropertyChanged += OnPropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (CustomObject3D obj in e.OldItems)
                {
                    obj.PropertyChanged -= OnPropertyChanged;
                }
            }

            OnCollectionChanged(e);
        }

        //=============== CollectionChanged Event ===============
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
        #endregion


        /// <summary>
        /// Adds a 3d object to the scene 3d dictionary
        /// </summary>
        public void AddObject(CustomObject3D obj) => loadedObjects3D.Add(obj);

        /// <summary>
        /// Removes a 3d object from the scene collection by corresponding guid
        /// </summary>
        public void RemoveObject(Guid id)
        {
            var obj = loadedObjects3D.FirstOrDefault(x => x.Id == id);
            if (obj!=null)
                loadedObjects3D?.Remove(obj);
        }

        /// <summary>
        /// Removes a 3d object from the scene collection by corresponding tag 
        /// </summary>
        public void RemoveObject(string tag)
        {
            foreach (var obj in loadedObjects3D)
            {
                if (obj.Tag.Equals(tag))
                    loadedObjects3D?.Remove(obj);
            }
        }

        /// <summary>
        /// Empties a scene 3d object collection
        /// </summary>
        public void RemoveAll() => loadedObjects3D.Clear();


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
            }
        }



        public void SetupLighting()
        {
            var ambientLight = new ModelVisual3D
            {
                Content = new AmbientLight(Colors.White)
            };

            CustomObject3D ambientlight = new(ambientLight, tag: "light");

            AddObject(ambientlight);
        }

    }
}
