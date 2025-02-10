This project is based on DanbooruDownloader by KichangKim

Licensed under the MIT License.

Original repository: https://github.com/KichangKim/DanbooruDownloader

# DanbooruDownloader
**DanbooruDownloader** is image download software for [Danbooru](https://danbooru.donmai.us/).

## Quick start
```
> DanbooruDownloader dump MyDataset
```
Download all images and its metadata on Danbooru server to local folder `MyDataset`. To see further help, run with `--help` option.

### Examples

```
> DanbooruDownloader dump -s 1 -e 10000
```
Download all images with id between 1 and 10,000

```
> DanbooruDownloader dump --use-paging -sp 1 -ep 400 --query "score:>=100 blonde_hair" --ext "png,jpg,mp4"
```
Download all images with a score of 100 or higher, containing the 'blonde_hair' tag, and having a file extension of PNG, JPG, or MP4. (with page limit 400)

## Output
Your downloaded images are saved as following structure.
```
MyDataset/
├── images/
│   ├── 00/
│   │   ├── 00000000000000000000000000000000.jpg
│   │   ├── 00000000000000000000000000000000-danbooru.json
│   │   ├── ...
│   ├── 01/
│   │   ├── ...
│   └── ff/
│       ├── ...
└── danbooru.sqlite
```
The filename of images is its MD5 hash. And `-danbooru.json` file contains the metadata of image post.

All of metadata is also saved as SQLite database, named `danbooru.sqlite`. Its table structure is same to the output of [Danbooru json API](https://danbooru.donmai.us/wiki_pages/43568).
