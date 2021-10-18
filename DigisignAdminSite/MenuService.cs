using System;
public class MenuService
{
    private string _id {get;set;}
    public string LastEditID { 
        get { return _id;}
        set {
            _id = value;
            NotifyStateChanged();
        }
    }
    
    public event Action OnChange;
    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}