using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Storage;
using System;
using Windows.UI.Popups;
using System.Threading.Tasks;

namespace MajaUWP.Office
{
    class TodoHandler
    {
        List<TodoItem> todoList = new List<TodoItem>();

        public ObservableCollection<TodoItem> OpenList() {

            //remove later!!!!!!!!
            ObservableCollection<TodoItem> opened = new ObservableCollection<TodoItem>();

            opened.Add(new TodoItem("Finnland einnehmen", TodoItem.UrgencyStates.highPriority));
            opened.Add(new TodoItem("Finnland auf der Karte finden", TodoItem.UrgencyStates.normalPriority));
            opened.Add(new TodoItem("Überprüfen ob es sich lohnt Finnland einzunehmen", TodoItem.UrgencyStates.lowPriority));

            return opened;
        }

        public async Task<bool>saveTodoList(ObservableCollection<TodoItem> toSave) {

            string jsonString = JsonConvert.SerializeObject(toSave);


            if (!string.IsNullOrEmpty(jsonString))
            {
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                StorageFile fileCreated = await folder.CreateFileAsync("todo.json", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(fileCreated, jsonString);
                return true;
            }
            else throw new Exception("Error in saving the todoList");



        }


        public async Task<bool>AddToSavedList(TodoItem toAdd)
        {
            
            ObservableCollection<TodoItem> list = await loadList();
            list.Add(toAdd);
            await saveTodoList(list);
            return true;
        }


        public async Task<ObservableCollection<TodoItem>> loadList()
        {
            try
            {
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.GetFileAsync("todo.json");
                string jsonString = await FileIO.ReadTextAsync(file);

                ObservableCollection<TodoItem> loadedList = JsonConvert.DeserializeObject<ObservableCollection<TodoItem>>(jsonString);

                return loadedList;

            }
            catch (System.IO.FileNotFoundException)
            {
                return new ObservableCollection<TodoItem>();
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message);
                var result = await dialog.ShowAsync();
                return null;
            }

            
            

        }
        





    }

    public class TodoItem
    {
        public string Text { get; set; }
        public bool IsDone { get; set; } = false; 
        public UrgencyStates Urgency { get; set; }
        public TodoItem(string _text, UrgencyStates _urgency)
        {
            Text = _text;
            Urgency = _urgency;
        }


        public enum UrgencyStates
        {
            lowPriority = 0,
            normalPriority = 1,
            highPriority = 2
        }
    }
}
