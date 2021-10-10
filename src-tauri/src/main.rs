#![cfg_attr(all(not(debug_assertions), target_os = "windows"), windows_subsystem = "windows")]

use crate::store::build_modules;
use crate::store::modules::config::commands::fetch_config;

mod menu;
mod store;

#[derive(serde::Serialize)]
struct CustomResponse {
    message: String,
}

fn main() {
    let mut builder = tauri::Builder::default();

    builder = build_modules(builder);

    builder
        .invoke_handler(tauri::generate_handler![fetch_config])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
