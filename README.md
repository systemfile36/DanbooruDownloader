This project is based on DanbooruDownloader by KichangKim

Licensed under the MIT License.

Original repository: https://github.com/KichangKim/DanbooruDownloader

I upgrade original DanbooruDownloader for download and manage dataset from Danbooru for deep-learning.

Some options(`--ext`, `--query`, paging options, etc) is added

Somae commands(`clean`, etc) will be added

# DanbooruDownloader
**DanbooruDownloader** is image download software for [Danbooru](https://danbooru.donmai.us/).

## Quick start
```
> DanbooruDownloader dump MyDataset
```
Download all images and its metadata on Danbooru server to local folder `MyDataset`. To see further help, run with `--help` option.

### `dump` Examples

```
> DanbooruDownloader dump <path> -s 1 -e 10000
```
Download all images with id between 1 and 10,000

```
> DanbooruDownloader dump <path> \
  --use-paging -sp 1 -ep 400 --query "score:>=100 blonde_hair" \
  --ext "png,jpg,mp4"
```
Download all images with a score of 100 or higher, containing the 'blonde_hair' tag, and having a file extension of PNG, JPG, or MP4. (with page limit 400)

```
> DanbooruDownloader dump <path> \
  --use-paging -sp 1 -ep 1000 --limit 100 --query "score:>=200 order:id_desc" \
  --ext "png,jpg"
```

Download all images with a score of 200 or higher, having a file extension of PNG, JPG. (with page beetween 1 and 1,000. posts per page is 100 (--limit option))

```
> DanbooruDownloader dump <path> \
  --use-paging -sp 1 -ep 10 --limit 100 --query "score:>=200 blonde_hair" \
  --resizing --resize-size "512x512" --resize-threshold 200
```

Download all images with a score of 200 or higher, containing the 'blonde_hair' tag. If the file size of image exceeds 200, it will be resized to 512x512. 

### `clean` Examples

```
> DanbooruDownloader clean <path> "tag_string LIKE '%blonde_hair%'"
```

Deletes records and image files that contain 'blonde_hair' tag. 

```
> DanbooruDownloader clean <path> "score < 100" --db-name "myData.sqlite"
```

Deletes records and image files that match "score < 100" with specific db name. (default is 'danbooru.sqlite')

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

All of metadata is also saved as SQLite database, named `danbooru.sqlite`. Its table structure is same to the output of [Danbooru json API](https://danbooru.donmai.us/wiki_pages/help:api). 
except for 'media_asset'.
