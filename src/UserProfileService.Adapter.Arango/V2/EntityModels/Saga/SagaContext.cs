#if NETCOREAPP3_1
using System;
using Chronicle;
using Newtonsoft.Json;
using UserProfileService.Common.V2.Abstractions;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.Saga
{
    public class SagaContext
    {
        private static JsonSerializerSettings _SerializerSettings;

        public SagaContext(IJsonSerializerSettingsProvider serializerSettingsProvider = null)
        {
            _SerializerSettings = serializerSettingsProvider?.GetNewtonsoftSettings() ?? new JsonSerializerSettings();
        }

        public ContextSagaLogData TransformForContext(ISagaLogData logData)
        {
            return new ContextSagaLogData
            {
                Id = logData.Id,
                TypeName = logData.Type.AssemblyQualifiedName,
                MessageTypeName = logData.Message?.GetType().AssemblyQualifiedName ?? string.Empty,
                CreatedAt = logData.CreatedAt,
                SerializedMessage = JsonConvert.SerializeObject(logData.Message, _SerializerSettings)
            };
        }

        public ContextSagaState TransformForContext(ISagaState state)
        {
            return new ContextSagaState
            {
                Id = state.Id,
                TypeName = state.Type.AssemblyQualifiedName,
                DataTypeName = state.Data?.GetType().AssemblyQualifiedName ?? string.Empty,
                // ¯\_(ツ)_/¯
                State = state.State,
                SerializedData = JsonConvert.SerializeObject(state.Data, _SerializerSettings)
            };
        }

        public ISagaLogData TransformFromContext(ContextSagaLogData logData)
        {
            return new CustomSagaLogData(
                logData.Id,
                Type.GetType(logData.TypeName),
                logData.CreatedAt,
                string.IsNullOrWhiteSpace(logData.SerializedMessage)
                    ? null
                    : JsonConvert.DeserializeObject(
                        logData.SerializedMessage,
                        Type.GetType(logData.MessageTypeName),
                        _SerializerSettings));
        }

        public ISagaState TransformFromContext(ContextSagaState state)
        {
            return new CustomSagaState(
                state.Id,
                Type.GetType(state.TypeName),
                state.State,
                string.IsNullOrWhiteSpace(state.DataTypeName)
                    ? null
                    : JsonConvert.DeserializeObject(
                        state.SerializedData,
                        Type.GetType(state.DataTypeName),
                        _SerializerSettings));
        }

        /// <summary>
        ///     SQL-DB-Appropriate container for <see cref="ISagaLogData" />.
        /// </summary>
        public class ContextSagaLogData
        {
            /// <summary>
            ///     <see cref="ISagaLogData.CreatedAt" />
            /// </summary>
            public long CreatedAt { get; set; }

            /// <summary>
            ///     String representation of <see cref="ISagaLogData.Id" />
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            ///     AssemblyQualifiedType of <see cref="ISagaLogData.Message" />
            /// </summary>
            public string MessageTypeName { get; set; }

            /// <summary>
            ///     Json-serialized data of <see cref="ISagaLogData.Message" />
            /// </summary>
            public string SerializedMessage { get; set; }

            /// <summary>
            ///     Assembly-name of <see cref="ISagaLogData.Type" />
            /// </summary>
            public string TypeName { get; set; }
        }

        /// <summary>
        ///     SQl-DB-Appropriate container for <see cref="ISagaState" />.
        /// </summary>
        public class ContextSagaState
        {
            /// <summary>
            ///     Assembly-name of <see cref="ISagaState.Data" />
            /// </summary>
            public string DataTypeName { get; set; }

            /// <summary>
            ///     String representation of <see cref="ISagaState.Id" />
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            ///     Json-serialized data of <see cref="ISagaState.Data" />.
            /// </summary>
            public string SerializedData { get; set; }

            /// <summary>
            ///     <see cref="ISagaState.State" />
            /// </summary>
            public SagaStates State { get; set; }

            /// <summary>
            ///     Assembly-name of <see cref="ISagaState.Type" />.
            /// </summary>
            public string TypeName { get; set; }
        }

        /// <inheritdoc />
        private class CustomSagaLogData : ISagaLogData
        {
            /// <inheritdoc />
            public long CreatedAt { get; }

            /// <inheritdoc />
            public SagaId Id { get; }

            /// <inheritdoc />
            public object Message { get; }

            /// <inheritdoc />
            public Type Type { get; }

            /// <summary>
            ///     create a new instance of this implementation of <see cref="ISagaLogData" />
            /// </summary>
            /// <param name="sagaId"></param>
            /// <param name="sagaType"></param>
            /// <param name="createdAt"></param>
            /// <param name="message"></param>
            public CustomSagaLogData(SagaId sagaId, Type sagaType, long createdAt, object message)
            {
                (Id, Type, CreatedAt, Message) = (sagaId, sagaType, createdAt, message);
            }
        }

        /// <inheritdoc />
        private class CustomSagaState : ISagaState
        {
            /// <inheritdoc />
            public object Data { get; private set; }

            /// <inheritdoc />
            public SagaId Id { get; }

            /// <inheritdoc />
            public SagaStates State { get; private set; }

            /// <inheritdoc />
            public Type Type { get; }

            /// <summary>
            ///     Create a new instance of this implementation of <see cref="ISagaState" />.
            /// </summary>
            /// <param name="id"></param>
            /// <param name="type"></param>
            /// <param name="state"></param>
            /// <param name="data"></param>
            public CustomSagaState(SagaId id, Type type, SagaStates state, object data)
            {
                (Id, Type, State, Data) = (id, type, state, data);
            }

            /// <inheritdoc />
            public void Update(SagaStates state, object data = null)
            {
                (State, Data) = (state, data);
            }
        }
    }
}
#endif
