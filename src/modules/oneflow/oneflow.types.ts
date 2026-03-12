export type OneFlowHttpMethod = "GET" | "POST";

export type OneFlowRequestOptions = {
  method: OneFlowHttpMethod;
  path: string;
  query?: Record<string, string | number | undefined>;
  body?: unknown;
};

export type OneFlowResponse<T = unknown> = {
  status: number;
  data: T;
};
