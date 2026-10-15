import accountApiInstance from '@/api/accountInstance.js'

// 桌位相關 API ===========================================================

/// <summary>
/// 取得指定預約桌
/// </summary>
export const getDiningTable = (diningTableId) =>
  accountApiInstance.get('/DiningTable/GetDiningTable', {
    params: { diningTableId },
  })

/// <summary>
/// 取得預約桌列表
/// </summary>
export const getDiningTables = () => accountApiInstance.get('/DiningTable/GetDiningTables')

/// <summary>
/// 新增預約桌
/// </summary>
export const createDiningTable = (request) =>
  accountApiInstance.post('/DiningTable/CreateDiningTable', request)

/// <summary>
/// 停用預約桌
/// </summary>
export const cancelDiningTable = (diningTableId) =>
  accountApiInstance.put('/DiningTable/CancelDiningTable', null, {
    params: { diningTableId },
  })

/// <summary>
/// 啟用預約桌
/// </summary>
export const enableDiningTable = (diningTableId) =>
  accountApiInstance.put('/DiningTable/EnableDiningTable', null, {
    params: { diningTableId },
  })
