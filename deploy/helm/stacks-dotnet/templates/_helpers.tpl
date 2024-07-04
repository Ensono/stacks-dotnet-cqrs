{{/*
Expand the name of the chart.
*/}}
{{- define "stacks-dotnet.name" -}}
{{- .Chart.Name | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "stacks-dotnet.fullname" -}}
{{- printf "%s-%s" .Release.Name .Chart.Name | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "stacks-dotnet.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "stacks-dotnet.labels" -}}
helm.sh/chart: {{ include "stacks-dotnet.chart" . }}
{{ include "stacks-dotnet.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "stacks-dotnet.selectorLabels" -}}
app.kubernetes.io/component: {{ .Values.app.component }}
app.kubernetes.io/name: {{ .Values.app.name }}
app.kubernetes.io/part-of: {{ .Values.app.project }}
environment: {{ .Values.env }}
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "stacks-dotnet.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "stacks-dotnet.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}


{{- define "stacks-dotnet.secretName" -}}
{{- printf "%s-%s" (include "stacks-dotnet.name" .) "secret" }}
{{- end }}

{{/*
Common annotations
*/}}
{{- define "stacks-dotnet.annotations" -}}
app.kubernetes.io/version: {{ .Chart.AppVersion }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/instance: {{- printf " %s-%s" .Values.app.project .Values.app.name }}
applicationId: {{- printf " %s/%s" .Values.app.project .Values.app.name }}
applicaitonName: {{- printf " %s-%s" .Values.app.project .Values.app.name }}
customerID: {{ .Values.app.company }}
owner: {{- printf " %s/%s" .Values.app.company .Values.app.project }}
projectID: {{ .Values.app.project }}
version: {{ .Chart.AppVersion }}
{{- end }}


{{/*
Common labels for Worker Resources
*/}}
{{- define "stacks-dotnet-worker.labels" -}}
helm.sh/chart: {{ include "stacks-dotnet.chart" . }}
{{ include "stacks-dotnet-worker.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels for Worker Resources
*/}}
{{- define "stacks-dotnet-worker.selectorLabels" -}}
app.kubernetes.io/component: {{ .Values.app.component }}
app.kubernetes.io/name: {{- printf " %s-%s" .Values.app.name .Values.app.workersuffix }}
app.kubernetes.io/part-of: {{ .Values.app.project }}
environment: {{ .Values.env }}
{{- end }}


{{/*
Common annotations for Worker Resources
*/}}
{{- define "stacks-dotnet-worker.annotations" -}}
app.kubernetes.io/version: {{ .Chart.AppVersion }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/instance: {{- printf " %s-%s-%s" .Values.app.project .Values.app.name .Values.app.workersuffix}}
applicationId: {{- printf " %s/%s-%s" .Values.app.project .Values.app.name .Values.app.workersuffix}}
applicaitonName: {{- printf " %s-%s-%s" .Values.app.project .Values.app.name .Values.app.workersuffix}}
customerID: {{ .Values.app.company }}
owner: {{- printf " %s/%s" .Values.app.company .Values.app.project }}
projectID: {{ .Values.app.project }}
version: {{ .Chart.AppVersion }}
{{- end }}

{{/*
Create a default fully qualified app name for worker resources
*/}}
{{- define "stacks-dotnet-worker.fullname" -}}
{{- printf "%s-%s-%s" .Release.Name .Chart.Name "worker" | trunc 63 | trimSuffix "-" }}
{{- end }}