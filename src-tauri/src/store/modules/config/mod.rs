pub mod commands;

#[derive(serde::Serialize, Clone)]
pub struct ConfigState {
    game_location: Option<String>,

    game_hashtable_path: String,
    lcu_hashtable_path: String,

    game_hashtable_checksum: Option<String>,
    lcu_hashtable_checksum: Option<String>,

    packed_bin_regex: String,
    packed_bin_keyowrds: Vec<String>,

    generate_hashes_from_bin: bool,
    sycn_hashes: bool,
}

impl Default for ConfigState {
    fn default() -> Self {
        Self {
            game_location: None,

            game_hashtable_path: String::from("GAME_HASHTABLE.txt"),
            lcu_hashtable_path: String::from("LCU_HASHTABLE.txt"),

            game_hashtable_checksum: None,
            lcu_hashtable_checksum: None,

            packed_bin_regex: String::from(r"^DATA/.*_(Skins_Skin|Tiers_Tier|(Skins|Tiers)_Root).*\.bin$"),
            packed_bin_keyowrds: vec![String::from("Skins"), String::from("Tiers")],

            generate_hashes_from_bin: false,
            sycn_hashes: true,
        }
    }
}
