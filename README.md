
# SH2 File Utility

A small file manipulation utility for Soul Hackers 2

## Usage

```txt
Description:
  SH2 File Utility

Usage:
  sh2fileutil [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  save <input> <output>   (De)compress game saves
  crypt <input> <output>  (En/De)crypt game assets
```

### Save Command

```txt
Description:
  (De)compress game saves

Usage:
  sh2fileutil save [<input> [<output>]] [options]

Arguments:
  <input>   Input path
  <output>  Output path
```

- Saves are gzipped json files
- The default save path for the steam version is `%AppData%\Roaming\SEGA\SOULHACKERS2\Steam\<steam_id>\SaveData`
- Examples:
  - To decompress a save:

    ```txt
    sh2fileutil save data.dat data.json
    ```

  - To compress a save:

    ```txt
    sh2fileutil save data.json data.dat
    ```

### Crypt Command

```txt
Description:
  (En/De)crypt game assets

Usage:
  sh2fileutil crypt [<input> [<output>]] [options]

Arguments:
  <input>   Input path
  <output>  Output path

Options:
  -m, --mode <AssetDec|AssetEnc|Ignore>  Crypt mode [default: AssetDec]
  -k, --key <key>                        Crypt key
```

- Encrypted game assets:
  - `StreamingAssets/win/Message/` (all asset bundles)
  - `StreamingAssets/win/Table/commontable` (asset bundle)
  - `StreamingAssets/win/Version.txt` (json)
- By default, this command checks if the file being encrypted/decrypted is a unity asset - you can bypass this behavior by using the `-m Ignore` option
- The encryption/decryption key is the **original** file name
- Examples:
  - To decrypt an encrypted asset:

    ```txt
    sh2fileutil crypt commontable commontable.dec -m AssetDec
    ```

  - To encrypt a decrypted asset:

    ```txt
    sh2fileutil crypt commontable.dec commontable -m AssetEnc -k commontable
    ```
