using System;
using Microsoft.Practices.ObjectBuilder2.Tests.TestDoubles;
using Microsoft.Practices.ObjectBuilder2.Tests.TestObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.ObjectBuilder2.Tests
{
    [TestClass]
    public class InternalAndPrivatePlanFixture
    {
        [TestMethod]
        public void ExistingObjectIsUntouchedByConstructionPlan()
        {
            TestingBuilderContext context = GetContext();
            IBuildPlanPolicy plan = GetPlanCreator(context).CreatePlan(context, typeof(OptionalLogger));

            OptionalLogger existing = new OptionalLogger("C:\\foo.bar");

            context.BuildKey = typeof(OptionalLogger);
            context.Existing = existing;

            plan.BuildUp(context);
            object result = context.Existing;

            Assert.AreSame(existing, result);
            Assert.AreEqual("C:\\foo.bar", existing.LogFile);
        }

        [TestMethod]
        public void CanCreateObjectWithoutExplicitConstructorDefined()
        {
            TestingBuilderContext context = GetContext();
            IBuildPlanPolicy plan =
                GetPlanCreator(context).CreatePlan(context,
                    typeof(InternalObjectWithoutExplicitConstructor));

            context.BuildKey = typeof(InternalObjectWithoutExplicitConstructor);
            plan.BuildUp(context);
            InternalObjectWithoutExplicitConstructor result =
                (InternalObjectWithoutExplicitConstructor)context.Existing;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(MethodAccessException))]
        public void CannotCreatePlanForPrivateClass()
        {
            TestingBuilderContext context = GetContext();
            IBuildPlanPolicy plan =
                GetPlanCreator(context).CreatePlan(context,
                    typeof(PrivateClassWithoutExplicitConstructor));

            context.BuildKey = typeof(PrivateClassWithoutExplicitConstructor);
            plan.BuildUp(context);
        }

        private TestingBuilderContext GetContext()
        {
            StagedStrategyChain<BuilderStage> chain = new StagedStrategyChain<BuilderStage>();
            chain.AddNew<DynamicMethodConstructorStrategy>(BuilderStage.Creation);

            DynamicMethodBuildPlanCreatorPolicy policy =
                new DynamicMethodBuildPlanCreatorPolicy(chain);

            TestingBuilderContext context = new TestingBuilderContext();

            context.Strategies.Add(new LifetimeStrategy());

            context.PersistentPolicies.SetDefault<IConstructorSelectorPolicy>(
                new ConstructorSelectorPolicy<InjectionConstructorAttribute>());
            context.PersistentPolicies.SetDefault<IDynamicBuilderMethodCreatorPolicy>(
                DynamicBuilderMethodCreatorFactory.CreatePolicy());
            context.PersistentPolicies.SetDefault<IBuildPlanCreatorPolicy>(policy);

            return context;
        }

        private IBuildPlanCreatorPolicy GetPlanCreator(IBuilderContext context)
        {
            return context.Policies.Get<IBuildPlanCreatorPolicy>(null);
        }
        internal class InternalObjectWithoutExplicitConstructor
        {
            public void DoSomething()
            {
                // We do nothing!
            }
        }

        private class PrivateClassWithoutExplicitConstructor
        {
            public void DoNothing()
            {
                // Again, do nothing
            }
        }
    }
}
