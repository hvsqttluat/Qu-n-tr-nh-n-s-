export interface SystemStatus {
  cpu: string;
  memory: string;
  uptime: number;
  activeUsers: number;
  securityLevel: string;
}

export interface CommandRequest {
  command: string;
  parameters?: any;
}

export interface CommandResponse {
  status: string;
  executionId: string;
  data: any;
  message: string;
}
