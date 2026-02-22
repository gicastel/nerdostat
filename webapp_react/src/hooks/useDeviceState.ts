import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "../services/api";
import { APIMessage } from "../types/models";

export const QUERY_KEYS = {
  deviceState: ["deviceState"] as const,
  program: ["program"] as const,
};

export function useDeviceState(refetchInterval: number = 5 * 60 * 1000) {
  return useQuery<APIMessage, Error>({
    queryKey: QUERY_KEYS.deviceState,
    queryFn: () => api.getDeviceState(),
    refetchInterval,
    retry: 3,
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
  });
}

export function useSetManualSetpoint() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      temperature,
      durationMinutes,
      overrideWithoutExpiry = false,
    }: {
      temperature: number;
      durationMinutes: number | null;
      overrideWithoutExpiry?: boolean;
    }) =>
      api.setManualSetpoint(
        temperature,
        durationMinutes,
        overrideWithoutExpiry,
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.deviceState });
    },
  });
}

export function useClearSetpoint() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => api.clearSetpoint(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.deviceState });
    },
  });
}

export function useAwayMode() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (enabled: boolean) => api.setAwayMode(enabled),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.deviceState });
    },
  });
}

export function useProgram() {
  return useQuery({
    queryKey: QUERY_KEYS.program,
    queryFn: () => api.getProgram(),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

export function useUpdateProgram() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: api.updateProgram,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.program });
    },
  });
}
