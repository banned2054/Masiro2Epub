好的，让我整理一下这个 API 的完整信息：

---

## 真白萌小说搜索 API 完整文档

### 基础信息

| 项目             | 内容                    |
| ---------------- | ----------------------- |
| **基础 URL**     | `https://masiro.me`     |
| **API 端点**     | `/admin/loadMoreNovels` |
| **请求方法**     | `GET`                   |
| **Content-Type** | `application/json`      |

### 请求 URL 格式

```
GET https://masiro.me/admin/loadMoreNovels?page={page}&keyword={keyword}&tags={tags}&tags_inverse={tags_inverse}&status={status}&ori={ori}&order={order}&author={author}&translator={translator}&collection={collection}
```

### Query 参数

| 参数           | 类型    | 必填 | 说明                                  |
| -------------- | ------- | ---- | ------------------------------------- |
| `page`         | integer | ✅    | 页码，从 1 开始                       |
| `keyword`      | string  | ❌    | 搜索关键词（书名/作者/翻译者）        |
| `tags`         | string  | ❌    | 标签 ID，多个用逗号分隔，如 `5,6,11`  |
| `tags_inverse` | string  | ❌    | 排除标签 ID，多个用逗号分隔           |
| `status`       | integer | ❌    | 状态：`0`=连载中, `1`=已完结          |
| `ori`          | integer | ❌    | 类型：`0`=日轻(翻译), `1`=原创        |
| `order`        | string  | ❌    | 排序方式，如 `hot`, `new`, `thumb_up` |
| `author`       | string  | ❌    | 按作者筛选                            |
| `translator`   | string  | ❌    | 按翻译者筛选                          |
| `collection`   | string  | ❌    | 按合集 ID 筛选                        |

### 请求 Headers

```http
GET /admin/loadMoreNovels?page=1 HTTP/1.1
Host: masiro.me
Cookie: 你的登录 Cookie（必须登录才能访问）
X-CSRF-TOKEN: ms5n5xdho0GTSBnOprQIxXmDvhBxe8OlYZviBe0W
Accept: application/json, text/javascript, */*; q=0.01
X-Requested-With: XMLHttpRequest
Referer: https://masiro.me/admin/novels
```

> ⚠️ **注意**：从代码中可以看到这个 API 需要登录（有 CSRF Token 验证），所以请求时必须携带登录 Cookie。

### 响应格式

**成功响应 (200 OK)**：

```json
{
  "novels": [
    {
      "id": 123,
      "title": "小说标题",
      "author": "作者名",
      "cover_img": "https://masiro.me/cover/xxx.jpg",
      "words": 100000,
      "hs": 50,
      "comment_nums": 100,
      "thumb_up": 50,
      "collect_nums": 200,
      "new_up_id": 456,
      "new_up_content": "最新章节标题",
      "is_ori": 0,
      "translators": [
        {
          "translator": 1,
          "name": "译者名"
        }
      ],
      "tags": [
        {
          "id": 5,
          "name": "异世界"
        }
      ],
      "lv_limit": 0
    }
  ],
  "pages": 10
}
```

### 响应字段说明

| 字段                      | 类型    | 说明                     |
| ------------------------- | ------- | ------------------------ |
| `novels`                  | array   | 小说列表                 |
| `novels[].id`             | integer | 小说 ID                  |
| `novels[].title`          | string  | 小说标题                 |
| `novels[].author`         | string  | 作者名                   |
| `novels[].cover_img`      | string  | 封面图片 URL             |
| `novels[].words`          | integer | 总字数                   |
| `novels[].hs`             | integer | 总话数/章节数            |
| `novels[].comment_nums`   | integer | 评论数                   |
| `novels[].thumb_up`       | integer | 点赞数                   |
| `novels[].collect_nums`   | integer | 收藏数                   |
| `novels[].new_up_id`      | integer | 最新章节 ID              |
| `novels[].new_up_content` | string  | 最新章节标题             |
| `novels[].is_ori`         | integer | `0`=日轻, `1`=原创       |
| `novels[].translators`    | array   | 翻译者列表（日轻才有）   |
| `novels[].tags`           | array   | 标签列表                 |
| `novels[].lv_limit`       | integer | 阅读等级限制，`0`=无限制 |
| `pages`                   | integer | 总页数                   |

### 使用示例

#### cURL
```bash
curl 'https://masiro.me/admin/loadMoreNovels?page=1&keyword=异世界&ori=0' \
  -H 'X-Requested-With: XMLHttpRequest' \
  -H 'Referer: https://masiro.me/admin/novels' \
  -b '你的登录Cookie'
```

#### JavaScript (Fetch)
```javascript
fetch('https://masiro.me/admin/loadMoreNovels?page=1&keyword=异世界&tags=5&status=0', {
  method: 'GET',
  headers: {
    'X-Requested-With': 'XMLHttpRequest',
    'Referer': 'https://masiro.me/admin/novels'
  },
  credentials: 'include'  // 携带 Cookie
})
.then(res => res.json())
.then(data => console.log(data));
```

#### Python (Requests)
```python
import requests

session = requests.Session()
# 先登录获取 Cookie
# session.post('https://masiro.me/login', data={...})

response = session.get(
    'https://masiro.me/admin/loadMoreNovels',
    params={
        'page': 1,
        'keyword': '异世界',
        'ori': 0,
        'tags': '5,6'
    },
    headers={
        'X-Requested-With': 'XMLHttpRequest',
        'Referer': 'https://masiro.me/admin/novels'
    }
)
data = response.json()
print(data)
```

### 标签 ID 参考（部分）

从 HTML 中提取的部分标签：

| ID  | 标签名    |
| --- | --------- |
| 5   | 异世界    |
| 6   | 迷宫地城  |
| 11  | 校园      |
| 24  | 恋爱/情感 |
| 25  | 科幻      |
| 40  | VR/游戏   |
| 45  | 现代      |
| 46  | 悬疑/推理 |
| 50  | 历史背景  |
| 54  | 末世/废土 |

---

还有什么需要补充的吗？比如错误响应处理或者分页逻辑？