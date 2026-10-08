/**
 * 取得 Enum 的描述
 * @param {Object} enumObj enums.js 中的 enum 物件
 * @param {Number} value enum 的值
 * @returns
 */
export const getEnumDescription = (enumObj, value) => {
  const key = Object.keys(enumObj).find((key) => enumObj[key].value === value)
  if (key) return enumObj[key].description
  else {
    console.error(`enum value '${value}' is undefined`)
    return value
  }
}

// Http 回傳狀態碼
export const httpCodeStatusEnum = Object.freeze({
  Ok: 200,
  BadRequest: 400,
  Unauthorized: 401,
  Forbidden: 403,
  NotFound: 404,
  MethodNotAllowed: 405,
  ManyRequest: 429,
  InternalServerError: 500,
  ServiceUnavailable: 503,
})

// 自訂回傳狀態碼
export const codeStatusEnum = Object.freeze({
  // 成功
  Success: 2000,
  // Request驗證失敗
  RequestError: 4000,
  // 查無資料
  NotFound: 4001,
  // 內部伺服器錯誤
  InternalException: 5000,
})
