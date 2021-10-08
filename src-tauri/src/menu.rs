// @todo: Address this section when this is worked out: https://github.com/tauri-apps/tauri/issues/2398
use tauri::{CustomMenuItem, Menu, MenuItem, Submenu};

pub fn get_menu() -> Menu {
  fn custom_menu(name: &str) -> CustomMenuItem {
    let c = CustomMenuItem::new(name.to_string(), name);
    return c;
  }
  let menu = Menu::new()
    .add_submenu(Submenu::new(
      // on macOS first menu is always app name
      "Tauri API",
      Menu::new()
        .add_native_item(MenuItem::About("Tauri".to_string()))
        .add_native_item(MenuItem::Separator)
        .add_native_item(MenuItem::Services)
        .add_native_item(MenuItem::Separator)
        .add_native_item(MenuItem::Hide)
        .add_native_item(MenuItem::HideOthers)
        .add_native_item(MenuItem::ShowAll)
        .add_native_item(MenuItem::Separator)
        .add_native_item(MenuItem::Quit),
    ))
    .add_submenu(Submenu::new(
      "File",
      Menu::new()
        .add_item(custom_menu("Open...").accelerator("cmdOrControl+O"))
        .add_native_item(MenuItem::Separator)
        .add_item(custom_menu("Close").accelerator("cmdOrControl+W"))
        .add_item(custom_menu("Save").accelerator("cmdOrControl+S"))
        .add_item(custom_menu("Save As...").accelerator("shift+cmdOrControl+S")),
    ))
    .add_submenu(Submenu::new(
      "Help",
      Menu::new().add_item(custom_menu("Learn More")),
    ))
    .add_native_item(MenuItem::Copy);
  menu
}