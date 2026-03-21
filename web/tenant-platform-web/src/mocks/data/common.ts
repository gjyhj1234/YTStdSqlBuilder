export function ok<T>(data: T, message = 'operation.success') {
  return { code: 0, message, data }
}

export function fail(message = 'operation.failed', code = 1003) {
  return { code, message, data: null }
}

export function paged<T>(items: T[], page: number, pageSize: number) {
  const start = (page - 1) * pageSize
  const slice = items.slice(start, start + pageSize)
  return ok({
    items: slice,
    total: items.length,
    page,
    pageSize,
    totalPages: Math.ceil(items.length / pageSize),
  })
}

export function getPageParams(url: URL) {
  const page = Number(url.searchParams.get('Page') ?? url.searchParams.get('page') ?? '1')
  const pageSize = Number(url.searchParams.get('PageSize') ?? url.searchParams.get('pageSize') ?? '20')
  return { page, pageSize }
}
