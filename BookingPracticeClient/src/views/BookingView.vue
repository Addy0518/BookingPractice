<script setup>
import {
  getDiningTables,
  createDiningTable,
  cancelDiningTable,
  enableDiningTable,
} from '@/api/diningTableService'
import {
  getBookings,
  getAvailableBookings,
  createBooking,
  cancelBooking,
} from '@/api/bookingService'

const showToastSuccess = inject('showToastSuccess')
const showToastError = inject('showToastError')

// 後端 API 不論成敗 HTTP 多半是 200，要看 codeStatus
const isOk = (res) => res.data.codeStatus === 2000
const errMsg = (res) =>
  Object.values(res.data.error400 || {})
    .flat()
    .join('、') || '操作失敗'

const statusText = { 1: '預約成功', 3: '已取消' }

/* ---------- 桌位 ---------- */
const tables = ref([])
const newTable = ref({ tableName: '', capacity: 2, isActive: true })

const loadTables = async () => {
  const res = await getDiningTables()
  if (isOk(res)) tables.value = res.data.returnData
}

const addTable = async () => {
  const res = await createDiningTable(newTable.value)
  if (isOk(res)) {
    showToastSuccess('桌位已新增')
    newTable.value.tableName = ''
    await loadTables()
  } else showToastError(errMsg(res))
}

const deactivateTable = async (id) => {
  const res = await cancelDiningTable(id)
  if (isOk(res)) {
    showToastSuccess('桌位已停用')
    await loadTables()
  } else showToastError(errMsg(res))
}

const enableTable = async (id) => {
  const res = await enableDiningTable(id)
  if (isOk(res)) {
    showToastSuccess('桌位已啟用')
    await loadTables()
  } else showToastError(errMsg(res))
}

/* ---------- 訂位 ---------- */
const form = ref({
  diningTableId: null,
  date: '', // yyyy-MM-dd
  bookingTime: '', // 選到的時段 (ISO UTC)
  userName: '',
  userPhone: '',
  people: 2,
  note: '',
})
const slots = ref([])

// 選桌或日期後，載入可訂時段
const loadSlots = async () => {
  form.value.bookingTime = ''
  slots.value = []
  if (!form.value.diningTableId || !form.value.date) return
  // 以 UTC 的 00:00 當作那一天的起點（後端以 UTC 計算時段）
  const res = await getAvailableBookings(form.value.diningTableId, `${form.value.date}T00:00:00Z`)
  if (isOk(res)) slots.value = res.data.returnData
}

// 顯示用：UTC 轉成本地時間
const fmt = (iso) => new Date(iso.endsWith('Z') ? iso : iso + 'Z').toLocaleString('zh-TW')

const submitBooking = async () => {
  if (!form.value.bookingTime) return showToastError('請選擇時段')
  const res = await createBooking({
    userName: form.value.userName,
    userPhone: form.value.userPhone,
    diningTableId: form.value.diningTableId,
    people: form.value.people,
    bookingTime: form.value.bookingTime,
    note: form.value.note,
  })
  if (isOk(res)) {
    showToastSuccess('訂位成功')
    await Promise.all([loadSlots(), loadBookings()])
  } else {
    showToastError(errMsg(res)) // 例如「此時段已被預約！」
    await loadSlots() // 重新整理可訂時段
  }
}

/* ---------- 訂位列表 ---------- */
const bookings = ref([])
const filterDate = ref('')

const loadBookings = async () => {
  const res = await getBookings({
    pageIndex: 0,
    pageSize: 50,
    bookingTime: filterDate.value ? `${filterDate.value}T00:00:00Z` : null,
    bookingStatus: null,
  })
  if (isOk(res)) bookings.value = res.data.returnData
}

const cancel = async (id) => {
  const res = await cancelBooking(id)
  if (isOk(res)) {
    showToastSuccess('已取消')
    await Promise.all([loadBookings(), loadSlots()])
  } else showToastError(errMsg(res))
}

onMounted(async () => {
  await loadTables()
  await loadBookings()
})
</script>

<template>
  <div class="p-6 max-w-4xl mx-auto flex flex-col gap-8">
    <!-- 桌位管理 -->
    <section>
      <h2 class="text-lg font-bold mb-2">桌位管理</h2>
      <div class="flex gap-2 mb-3">
        <input v-model="newTable.tableName" placeholder="桌名" class="border px-2 py-1" />
        <input
          v-model.number="newTable.capacity"
          type="number"
          min="1"
          class="border px-2 py-1 w-20"
        />
        <button @click="addTable" class="px-3 py-1 bg-blue-500 text-white rounded">新增</button>
      </div>
      <ul>
        <li v-for="t in tables" :key="t.diningTableId" class="flex gap-3 py-1 border-b">
          <span class="w-24">{{ t.tableName }}</span>
          <span class="w-20">{{ t.capacity }} 人</span>
          <span class="w-16" :class="t.isActive ? 'text-green-500' : 'text-red-500'">{{
            t.isActive ? '啟用' : '停用'
          }}</span>
          <button
            v-if="t.isActive"
            @click="deactivateTable(t.diningTableId)"
            class="text-white bg-red-500 p-2"
          >
            停用
          </button>
          <button
            v-if="!t.isActive"
            @click="enableTable(t.diningTableId)"
            class="text-white bg-green-500 p-2"
          >
            啟用
          </button>
        </li>
      </ul>
    </section>

    <!-- 新增訂位 -->
    <section>
      <h2 class="text-lg font-bold mb-2">新增訂位</h2>
      <div class="flex flex-col gap-2 max-w-md">
        <select v-model="form.diningTableId" @change="loadSlots" class="border px-2 py-1">
          <option :value="null" disabled>選擇桌位</option>
          <option
            v-for="t in tables.filter((x) => x.isActive)"
            :key="t.diningTableId"
            :value="t.diningTableId"
          >
            {{ t.tableName }}（{{ t.capacity }} 人）
          </option>
        </select>
        <input v-model="form.date" @change="loadSlots" type="date" class="border px-2 py-1" />

        <div class="flex flex-wrap gap-2">
          <button
            v-for="s in slots"
            :key="s"
            @click="form.bookingTime = s"
            class="px-2 py-1 border rounded"
            :class="form.bookingTime === s ? 'bg-blue-500 text-white' : ''"
          >
            {{
              new Date(s.endsWith('Z') ? s : s + 'Z').toLocaleTimeString('zh-TW', {
                hour: '2-digit',
                minute: '2-digit',
              })
            }}
          </button>
          <span v-if="form.diningTableId && form.date && !slots.length" class="text-gray-400">
            這天沒有可訂時段
          </span>
        </div>

        <input v-model="form.userName" placeholder="姓名" class="border px-2 py-1" />
        <input v-model="form.userPhone" placeholder="電話" class="border px-2 py-1" />
        <input v-model.number="form.people" type="number" min="1" class="border px-2 py-1" />
        <input v-model="form.note" placeholder="備註（選填）" class="border px-2 py-1" />
        <button @click="submitBooking" class="px-3 py-1 bg-green-500 text-white rounded">
          送出訂位
        </button>
      </div>
    </section>

    <!-- 訂位列表 -->
    <section>
      <h2 class="text-lg font-bold mb-2">訂位列表</h2>
      <div class="flex gap-2 mb-3">
        <input v-model="filterDate" type="date" class="border px-2 py-1" />
        <button @click="loadBookings" class="px-3 py-1 border rounded">查詢</button>
      </div>
      <table class="w-full text-left">
        <thead>
          <tr>
            <th>時間</th>
            <th>桌位</th>
            <th>姓名</th>
            <th>人數</th>
            <th>狀態</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="b in bookings" :key="b.bookingId" class="border-t">
            <td>{{ fmt(b.bookingTime) }}</td>
            <td>{{ b.diningTableId }}</td>
            <td>{{ b.userName }}</td>
            <td>{{ b.people }}</td>
            <td>{{ statusText[b.bookingStatus] }}</td>
            <td>
              <button
                v-if="b.bookingStatus === 1"
                @click="cancel(b.bookingId)"
                class="text-red-500"
              >
                取消預約
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>
  </div>
</template>
