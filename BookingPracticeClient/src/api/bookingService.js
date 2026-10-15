import accountApiInstance from '@/api/accountInstance.js'

// 預訂相關 API ===========================================================

/// <summary>
/// 取得指定訂位資訊
/// </summary>
export const getBooking = (bookingId) =>
  accountApiInstance.get('/Booking/GetBooking', { params: { bookingId } })

/// <summary>
/// 取得訂位資訊列表
/// </summary>
export const getBookings = (request) => accountApiInstance.post('/Booking/GetBookings', request)

/// <summary>
/// 取得可預約的訂位時段
/// </summary>
export const getAvailableBookings = (diningTableId, date) =>
  accountApiInstance.get('/Booking/GetAvailableBookings', {
    params: { diningTableId: diningTableId, date: date },
  })

/// <summary>
/// 新增訂位
/// </summary>
export const createBooking = (request) => accountApiInstance.post('/Booking/CreateBooking', request)

/// <summary>
/// 取消訂位
/// </summary>
export const cancelBooking = (bookingId) =>
  accountApiInstance.put('/Booking/CancelBooking', null, { params: { bookingId } })
