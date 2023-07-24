using UnityEngine.UI;
using UserInterface;

namespace Custom_UI.InGame_UI
{
    public class UnitsIcon : MenuIcon
    {
        protected override void Start()
        {
            UIManager = UIManager.Instance;
            MyBtn = GetComponent<Button>();
        }
    }
}
