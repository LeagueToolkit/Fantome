use modules::config::ConfigState;
use tauri::{Builder, Wry};
pub mod modules;

pub(crate) fn build_modules(builder: Builder<Wry>) -> Builder<Wry> {
    builder.manage(ConfigState::default())
}
