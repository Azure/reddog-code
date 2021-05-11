module.exports = async function (context, ordersQueueItem) {

    // get order context from Rabbit
    context.log(`Debug: ${JSON.stringify(ordersQueueItem)}`);
    const obj = ordersQueueItem;
    const objData = obj? obj.data: {};
    const orderId = objData? objData.orderId: '';

    context.log(`OrderId: ${orderId}`);

    // output order to Azure service bus
    context.bindings.outputSbQueue = obj;
    context.log(`Order written to Azure Service Bus`);
    context.done();
};
