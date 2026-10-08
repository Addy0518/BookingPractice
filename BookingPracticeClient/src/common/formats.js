import zipCodeData from '@/assets/zipCodeData.json'

/**
 * 轉換台灣時區
 * @param {Date} value 時間
 * @returns LocaleString(zh-Hans-TW)
 */
export const formatTWDate = (value) => {
  if (value == null) {
    return ''
  } else {
    var localDate = new Date(value).toLocaleString('zh-Hans-TW')
    return localDate
  }
}

/**
 * 將日期轉換為整點
 * Example: param 2021-09-01 12:31:59.157 => return 2021-09-01 00:00:00.000
 * @param {Date | String} value
 * @returns 整點日期
 */
export const formatDateForZeroHour = (value) => {
  if (value == null) {
    return ''
  } else {
    var localDate = new Date(new Date(value).setHours(8, 0, 0, 0)) // setHours 偏移8小時為台灣時區
    return localDate
  }
}

/**
 * 轉為日期字串
 * Example: param 2021-09-01 12:31:59.157 => return 2021/09/01
 * @param {Date | String} value 時間
 * @returns yyyy/MM/dd
 */
export const formatDateString = (value) => {
  if (value == null) {
    return ''
  } else {
    const date = new Date(value)
    const year = date.getFullYear()
    // 月份+1，並確保格式為兩位數
    const month = ('0' + (date.getMonth() + 1)).slice(-2)
    // 日期，並確保格式為兩位數
    const day = ('0' + date.getDate()).slice(-2)
    // 按照 YYYY/MM/DD 的格式返回
    return `${year}/${month}/${day}`
  }
}

/**
 * 轉為日期時間字串
 * Example: param 2021-09-01 12:31:59.157 => return 2021/09/01 12:31
 * @param {Date | String} value 時間
 * @returns yyyy/MM/dd HH:mm
 */
export const formatDateTimeString = (value) => {
  const padZero = (num) => String(num).padStart(2, '0')

  const date = new Date(value)
  const year = date.getFullYear()
  const month = padZero(date.getMonth() + 1)
  const day = padZero(date.getDate())
  const hours = padZero(date.getHours())
  const minutes = padZero(date.getMinutes())

  return `${year}/${month}/${day} ${hours}:${minutes}`
}

/**
 * 轉換為 UTC+8 時區
 * @param {Date} value 時間
 * @returns {Date} UTC+8 時間
 */
export const formatUTC8Date = (value) => {
  const localDate = new Date(value)
  return new Date(localDate.getTime() - localDate.getTimezoneOffset() * 60000)
}

/**
 * 儲存時轉換為 YYYY-MM-DD 時間
 * @param {Date} value 時間
 * @returns {Date} YYYY-MM-DD 時間
 */
export const formatDateOnly = (value) => {
  if (!value) return null
  const d = new Date(value)
  const year = d.getFullYear()
  const month = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

/**
 * 拿到當月一號
 *
 * @returns {Date} 當月第一天 yyyy/MM/dd
 */
export const getFirstDayOfMonth = () => {
  const now = new Date()
  return new Date(now.getFullYear(), now.getMonth(), 1)
}
