export const testReports = [
  {
    id: "rpt_001",
    status: "VERIFIED",
    wasteTypes: ["RECYCLABLE"],
    wasteUnit: "SMALL_BAG",
    photos: ["https://picsum.photos/400/300?random=1", "https://picsum.photos/400/300?random=1", "https://picsum.photos/400/300?random=1"],
    address: "45 Phan Đăng Lưu, Phường 3, Bình Thạnh",
    description: "Khoảng 5kg chai nhựa và giấy carton",
    createdAt: new Date(Date.now() + 28 * 3600000).toISOString(),
    collectorName: "Tran Van Dung",
    pointReward: 100,
  },
  {
    id: "rpt_004",
    status: "REJECTED",
    wasteTypes: ["RECYCLABLE", "ORGANIC"],
    wasteUnit: "LARGE_BAG",
    photos: ["https://picsum.photos/400/300?random=4", "https://picsum.photos/400/300?random=9"],
    address: "23 Bạch Đằng, Phường 2, Bình Thạnh",
    description: "Nhiều loại rác khác nhau",
    createdAt: new Date(Date.now() - 28 * 3600000).toISOString(),
    collectorName: "Tran Dang Khoa",
    pointReward: 100
  },
];