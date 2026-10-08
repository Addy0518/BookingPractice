import accountApiInstance from '@/api/accountInstance.js'

// 使用者相關 API ===========================================================

/// <summary>
/// 使用者註冊
/// </summary>
export const registerAPI = (data) => accountApiInstance.post('/User/Register', data)
