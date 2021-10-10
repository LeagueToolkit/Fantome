use super::ConfigState;

#[tauri::command]
pub fn fetch_config(window: tauri::Window, config: tauri::State<'_, ConfigState>) -> ConfigState {
    config.inner().clone()
}
